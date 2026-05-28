using Exam.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
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
                    try
                    {
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Migrations applied successfully.");
                    }
                    catch (SqlException ex) when (ex.Number == 2714)
                    {
                        logger.LogWarning("Migrations skipped: Tables already exist. Attempting to synchronize migration history...");

                        // Manually create history table and insert the initial migration record
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                            BEGIN
                                CREATE TABLE [__EFMigrationsHistory] (
                                    [MigrationId] nvarchar(150) NOT NULL,
                                    [ProductVersion] nvarchar(32) NOT NULL,
                                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                                );
                            END
                            IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260410214138_InitialCreate')
                            BEGIN
                                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                                VALUES ('20260410214138_InitialCreate', '9.0.1');
                            END");

                        logger.LogInformation("Migration history synchronized successfully.");
                    }
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
