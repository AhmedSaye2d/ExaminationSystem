using Exam.Application.Dto.Common;
using FluentValidation;

namespace Exam.Application.Validation
{
    public interface IValidationService
    {
        Task<ServiceResponse> ValidateAsync<T>(T model, IValidator<T> validator);
    }
    public class ValidationService : IValidationService
    {
        public async Task<ServiceResponse> ValidateAsync<T>(T model, IValidator<T> validator)
        {
            var result = await validator.ValidateAsync(model);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResponse.Fail("Validation failed", errors);
            }

            return new ServiceResponse(true, "Validation successful");
        }
    }
}
