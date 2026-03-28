using Exam.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Exam.Infrastructure.Extensions
{
    public static class DatabaseInitializer
    {
        public static async Task ApplyMigrationsAndSeedAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                
                logger.LogInformation("Checking for pending migrations...");
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("No pending migrations found.");
                }

                // Ensure roles and admin are created
                logger.LogInformation("Seeding roles and admin user...");
                await DbSeeder.SeedRolesAndAdminAsync(services);
                
                // Seed test data
                logger.LogInformation("Seeding application data...");
                await DbSeeder.SeedDataAsync(services);
                
                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                // In production, we usually want to stop the app if the DB is not ready
                throw;
            }
        }
    }
}
