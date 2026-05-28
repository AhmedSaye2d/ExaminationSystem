namespace Exam.Application.Dto.Identity
{
    public record LoginResponse(
        bool Success,
        string Message,
        string? Token = null,
        string? RefreshToken = null,
        string? Name = null,
        string? Role = null,
        string? Email = null
    );
}
