using Exam.Application.Mapping;
using Exam.Application.Services.Implementation;
using Exam.Application.Services.Interfaces;
using Exam.Application.Services.Interfaces.Authentication;
using Exam.Application.Services.Interfaces.IChoiceServices;
using Exam.Application.Services.Interfaces.ICourseService;
using Exam.Application.Services.Interfaces.IDepartmentServices;
using Exam.Application.Services.Interfaces.IExamServices;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Exam.Application.Services.Interfaces.IStudentServices;
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
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IChoiceService, ChoiceService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IInstructorService, InstructorService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IStudentExamService, StudentExamService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            return services;

        }

    }
}



