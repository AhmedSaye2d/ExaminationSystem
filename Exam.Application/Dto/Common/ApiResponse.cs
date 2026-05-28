namespace Exam.Application.Dto.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, int statusCode, string message, T? data = default, IEnumerable<string>? errors = null)
        {
            Success = success;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>(true, statusCode, message, data);
        }

        public static ApiResponse<T> FailureResponse(string message, int statusCode = 400, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>(false, statusCode, message, default, errors);
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse(bool success, int statusCode, string message, object? data = default, IEnumerable<string>? errors = null)
            : base(success, statusCode, message, data, errors)
        {
        }

        public static ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
        {
            return new ApiResponse(true, statusCode, message);
        }

        public static ApiResponse FailureResponse(string message, int statusCode = 400, IEnumerable<string>? errors = null)
        {
            return new ApiResponse(false, statusCode, message, null, errors);
        }
    }
}
