using Microsoft.AspNetCore.Identity;
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
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            // БД создается через SQL-скрипт CreateDatabase.sql, миграции не используются
            // Создаем роли (это должно работать, если БД доступна)
            // ВАЖНО: Не прерываем запуск приложения, если роли не могут быть созданы
            try
            {
                await EnsureRolesCreatedAsync(roleManager, logger);
                logger.LogInformation("Роли успешно созданы/проверены.");
            }
            catch (Exception rolesEx)
            {
                logger.LogError(rolesEx, "Ошибка при создании ролей. Приложение может работать некорректно.");
                // Не бросаем исключение, чтобы приложение могло запуститься
            }
        }

        private static async Task EnsureRolesCreatedAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roles = new[] { ApplicationRoles.Admin, ApplicationRoles.Teacher, ApplicationRoles.Student };

            foreach (var role in roles)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation("Роль {Role} создана.", role);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Не удалось создать/проверить роль {Role}.", role);
                }
            }
        }
    }
}
