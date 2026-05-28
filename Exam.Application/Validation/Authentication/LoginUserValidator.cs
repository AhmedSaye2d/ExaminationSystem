using Exam.Application.Dto.Identity;
using FluentValidation;

namespace Exam.Application.Validation.Authentication
{
    internal class LoginUserValidator : AbstractValidator<Login>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}