using EntityFramework.Exceptions.SqlServer;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using Exam.Infrastructure.Data;
using Exam.Infrastructure.Middleware;
using Exam.Infrastructure.Repository.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;

namespace Exam.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ===================== DbContext =====================
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Default"),
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure();
                    })
                .UseExceptionProcessor()
            );

            // ===================== Identity =====================
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ===================== JWT Authentication =====================
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                        )
                    };
                });

            // ===================== Application Infrastructure Services =====================
            services.AddScoped<IUserManagement, UserManagement>();
            services.AddScoped<IRoleManagement, RoleManagement>();
            services.AddScoped<ITokenManagement, TokenManagement>();

            return services;
        }

        // ===================== Middleware =====================
        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }
}
