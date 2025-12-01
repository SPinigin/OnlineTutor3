using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Infrastructure.Data
{
    /// <summary>
    /// Инициализатор базы данных
    /// </summary>
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            try
            {
                // Проверяем, существует ли база данных
                var canConnect = await context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    logger.LogError("База данных OnlineTutor3 не существует или недоступна. " +
                                   "Пожалуйста, создайте базу данных через SQL-скрипт CreateDatabase.sql перед запуском приложения.");
                    throw new InvalidOperationException(
                        "База данных OnlineTutor3 не существует. Создайте её через SQL-скрипт CreateDatabase.sql");
                }

                // Применяем миграции только если БД существует
                // Важно: база данных OnlineTutor3 должна быть создана заранее через SQL-скрипт CreateDatabase.sql
                logger.LogInformation("Применение миграций к существующей базе данных...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Миграции успешно применены.");

                // Создаем роли
                await EnsureRolesCreatedAsync(roleManager);
                logger.LogInformation("Роли успешно созданы/проверены.");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 262)
            {
                // Ошибка "CREATE DATABASE permission denied"
                logger.LogError(sqlEx,
                    "Недостаточно прав для создания базы данных. " +
                    "Убедитесь, что база данных OnlineTutor3 создана через SQL-скрипт CreateDatabase.sql " +
                    "и пользователь имеет права на подключение к ней.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при инициализации базы данных");
                throw;
            }
        }

        private static async Task EnsureRolesCreatedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { ApplicationRoles.Admin, ApplicationRoles.Teacher, ApplicationRoles.Student };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
