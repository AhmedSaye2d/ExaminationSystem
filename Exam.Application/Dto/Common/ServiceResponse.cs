namespace Exam.Application.Dto.Common
{
    public record ServiceResponse(bool Success, string Message, IEnumerable<string>? Errors = null)
    {
        public static ServiceResponse Ok(string message = "Success")
            => new(true, message);

        public static ServiceResponse Fail(string message, IEnumerable<string>? errors = null)
            => new(false, message, errors);
    }

}
