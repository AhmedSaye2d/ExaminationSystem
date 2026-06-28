using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Exam.Host.WebSockets
{
    /// <summary>
    /// Handles the WebSocket endpoint:  WS /ws/proctoring/frame
    ///
    /// Protocol
    /// ────────
    ///   Upgrade:    GET /ws/proctoring/frame?token={jwt}&student_id={id}&exam_id={id}
    ///   Client→Srv: Binary WebSocket message  = raw JPEG/PNG frame bytes
    ///   Srv→Client: Text   WebSocket message  = JSON AI response (FastApiResponseDto)
    ///               or JSON error             = { "error": "...", "code": "..." }
    ///
    /// Authentication
    /// ──────────────
    ///   WebSocket upgrades cannot carry Authorization headers after the initial
    ///   HTTP handshake, so the JWT is passed as a ?token= query parameter.
    ///   The token is validated against the same parameters used by the regular
    ///   JwtBearer middleware (same key / issuer / audience from appsettings).
    ///
    /// Shared Business Logic
    /// ─────────────────────
    ///   All validation + AI forwarding is done via IProctoringProcessor, which
    ///   is the exact same path taken by POST /api/Proctoring/frame.
    ///   ProctoringService, database logging, and reporting are NOT touched.
    /// </summary>
    public class ProctoringWebSocketHandler
    {
        private readonly IProctoringProcessor _processor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProctoringWebSocketHandler> _logger;

        // Maximum single-frame size accepted over WebSocket (default 10 MB).
        private readonly int _maxFrameBytes;

        // JSON serialiser options — shared across connections (thread-safe).
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public ProctoringWebSocketHandler(
            IProctoringProcessor processor,
            IConfiguration configuration,
            ILogger<ProctoringWebSocketHandler> logger)
        {
            _processor      = processor;
            _configuration  = configuration;
            _logger         = logger;
            _maxFrameBytes  = _configuration.GetValue<int>("WebSocket:MaxFrameSizeBytes", 10_485_760); // 10 MB
        }

        // ── Entry point called from Program.cs ────────────────────────────────

        /// <summary>
        /// Validates the HTTP upgrade request, accepts the WebSocket, and starts
        /// the frame-receive / response-send loop.
        /// </summary>
        public async Task HandleAsync(HttpContext context, CancellationToken appStopping)
        {
            // 1. Must be a WebSocket upgrade request
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("This endpoint requires a WebSocket connection.");
                return;
            }

            // 2. Validate JWT from query string
            var tokenValidation = ValidateToken(context.Request.Query["token"]);
            if (!tokenValidation.Success)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync(tokenValidation.ErrorMessage ?? "Unauthorized");
                return;
            }

            // 3. Extract student identity from token claims
            var currentUserId = tokenValidation.UserId;

            // 4. Extract student_id / exam_id from query params
            if (!int.TryParse(context.Request.Query["student_id"], out var studentId) ||
                !int.TryParse(context.Request.Query["exam_id"],   out var examId))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Missing or invalid student_id / exam_id query parameters.");
                return;
            }

            // 5. Accept the WebSocket upgrade
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            _logger.LogInformation(
                "WebSocket connected — Student {StudentId}, Exam {ExamId}, User {UserId}",
                studentId, examId, currentUserId);

            // 6. Use a linked token so we stop on app shutdown OR client disconnect
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStopping);

            try
            {
                await ReceiveFrameLoopAsync(webSocket, studentId, examId, currentUserId, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "WebSocket session cancelled — Student {StudentId}, Exam {ExamId}", studentId, examId);
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                _logger.LogWarning(
                    "Client disconnected abruptly — Student {StudentId}, Exam {ExamId}", studentId, examId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled error in WebSocket session — Student {StudentId}, Exam {ExamId}",
                    studentId, examId);
            }
            finally
            {
                cts.Cancel(); // Ensure all pending work is cancelled

                if (webSocket.State == WebSocketState.Open ||
                    webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Session ended",
                            CancellationToken.None);
                    }
                    catch
                    {
                        // Suppress errors during close — connection may already be gone
                    }
                }

                _logger.LogInformation(
                    "WebSocket disconnected — Student {StudentId}, Exam {ExamId}", studentId, examId);
            }
        }

        // ── Frame receive / process / respond loop ────────────────────────────

        private async Task ReceiveFrameLoopAsync(
            WebSocket webSocket,
            int studentId,
            int examId,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            // Reusable buffer for incoming frame data
            var buffer = new byte[_maxFrameBytes];

            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                // ── Receive one complete binary message ────────────────────────
                WebSocketReceiveResult receiveResult;
                int totalBytesReceived = 0;

                using var frameBuffer = new MemoryStream();
                do
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), cancellationToken);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation(
                            "Client requested close — Student {StudentId}", studentId);
                        return; // Exit loop cleanly
                    }

                    if (receiveResult.MessageType != WebSocketMessageType.Binary)
                    {
                        // Ignore unexpected text messages during frame stream
                        _logger.LogDebug(
                            "Ignoring non-binary message from Student {StudentId}", studentId);
                        continue;
                    }

                    totalBytesReceived += receiveResult.Count;

                    if (totalBytesReceived > _maxFrameBytes)
                    {
                        _logger.LogWarning(
                            "Frame too large ({Bytes} bytes) — Student {StudentId}", totalBytesReceived, studentId);

                        await SendErrorAsync(webSocket, "Frame too large.", "FRAME_TOO_LARGE", cancellationToken);

                        // Drain the rest of the oversized message without processing it
                        while (!receiveResult.EndOfMessage)
                            receiveResult = await webSocket.ReceiveAsync(
                                new ArraySegment<byte>(buffer), cancellationToken);

                        frameBuffer.SetLength(0); // discard accumulated data
                        totalBytesReceived = 0;
                        break;
                    }

                    await frameBuffer.WriteAsync(
                        buffer, 0, receiveResult.Count, cancellationToken);

                } while (!receiveResult.EndOfMessage);

                if (frameBuffer.Length == 0) continue; // Oversized or empty — skip

                // ── Wrap bytes in IFormFile and process ────────────────────────
                frameBuffer.Position = 0;

                var formFile = new FormFileAdapter(
                    frameBuffer,
                    frameBuffer.Length,
                    "frame.jpg",
                    "image/jpeg");

                var result = await _processor.ProcessFrameAsync(
                    studentId, examId, formFile, currentUserId, cancellationToken);

                // ── Send response back to client ───────────────────────────────
                if (result.Success && result.Result != null)
                {
                    await SendResponseAsync(webSocket, result.Result, cancellationToken);
                }
                else
                {
                    await SendErrorAsync(
                        webSocket,
                        result.ErrorMessage ?? "Unknown error",
                        result.ErrorCode   ?? "UNKNOWN",
                        cancellationToken);

                    // Close the connection on hard errors (auth / session failures)
                    if (result.StatusCode is 403 or 400)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.PolicyViolation,
                            result.ErrorMessage,
                            CancellationToken.None);
                        return;
                    }
                    // For transient AI errors (502/504/500) keep connection alive
                    // so the frontend can retry the next frame
                }
            }
        }

        // ── Send helpers ──────────────────────────────────────────────────────

        private static async Task SendResponseAsync(
            WebSocket webSocket,
            FastApiResponseDto response,
            CancellationToken cancellationToken)
        {
            var json  = JsonSerializer.Serialize(response, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }

        private static async Task SendErrorAsync(
            WebSocket webSocket,
            string message,
            string code,
            CancellationToken cancellationToken)
        {
            var payload = new { error = message, code };
            var json    = JsonSerializer.Serialize(payload, _jsonOptions);
            var bytes   = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }

        // ── JWT validation ────────────────────────────────────────────────────

        private TokenValidationOutcome ValidateToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return TokenValidationOutcome.Fail("Missing authentication token.");

            var key     = _configuration["Jwt:Key"]!;
            var issuer  = _configuration["Jwt:Issuer"]!;
            var audience = _configuration["Jwt:Audience"]!;

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ClockSkew                = TimeSpan.Zero,
                ValidIssuer              = issuer,
                ValidAudience            = audience,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };

            try
            {
                var handler    = new JwtSecurityTokenHandler();
                var principal  = handler.ValidateToken(token, validationParams, out _);

                var userIdStr  = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out var userId))
                    return TokenValidationOutcome.Fail("Invalid user identity in token.");

                return TokenValidationOutcome.Valid(userId);
            }
            catch (SecurityTokenExpiredException)
            {
                return TokenValidationOutcome.Fail("Authentication token has expired.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("JWT validation failed: {Message}", ex.Message);
                return TokenValidationOutcome.Fail("Invalid authentication token.");
            }
        }

        // ── Inner types ───────────────────────────────────────────────────────

        private sealed record TokenValidationOutcome(bool Success, int UserId, string? ErrorMessage)
        {
            public static TokenValidationOutcome Valid(int userId) =>
                new(true, userId, null);

            public static TokenValidationOutcome Fail(string message) =>
                new(false, 0, message);
        }
    }

    // ── FormFile adapter ──────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a raw byte stream in an IFormFile so that the existing
    /// ProctoringService (which expects IFormFile) works unchanged.
    /// </summary>
    internal sealed class FormFileAdapter : IFormFile
    {
        private readonly Stream _stream;

        public FormFileAdapter(Stream stream, long length, string fileName, string contentType)
        {
            _stream     = stream;
            Length      = length;
            FileName    = fileName;
            ContentType = contentType;
            Name        = "frame";
            Headers     = new HeaderDictionary();
            ContentDisposition = $"form-data; name=\"frame\"; filename=\"{fileName}\"";
        }

        public string ContentType        { get; }
        public string ContentDisposition { get; }
        public IHeaderDictionary Headers { get; }
        public long Length               { get; }
        public string Name               { get; }
        public string FileName           { get; }

        public void CopyTo(Stream target)                                => _stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken ct = default) => _stream.CopyToAsync(target, ct);
        public Stream OpenReadStream()                                   => _stream;
    }
}
