using Exam.Application.Mapping;
using Exam.Application.Services.Implementation;
using Exam.Application.Services.Interfaces.Authentication;
using Exam.Application.Validation;
using Exam.Application.Validation.Authentication;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace Exam.Application.DependencyInjection
{
    public static class ServiceContiner
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingConfig));
            services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IAuthenticationServices, AuthenticationService>();


            return services;

        }

    }
}



