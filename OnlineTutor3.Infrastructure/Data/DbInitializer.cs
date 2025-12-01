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

            // Пытаемся применить миграции (если они есть)
            // Если БД уже создана через SQL-скрипт, миграции могут быть не нужны
            // ВАЖНО: Не прерываем запуск приложения, если миграции не могут быть применены
            try
            {
                logger.LogInformation("Попытка применения миграций...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Миграции успешно применены.");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 262)
            {
                // Ошибка "CREATE DATABASE permission denied" - БД уже существует, миграции не нужны
                logger.LogWarning("Миграции не применены (БД уже существует или недостаточно прав). Продолжаем работу...");
            }
            catch (InvalidOperationException ioEx)
            {
                // InvalidOperationException может возникать, если БД не существует или недоступна
                // Но мы знаем, что БД существует, поэтому просто пропускаем миграции
                logger.LogWarning(ioEx, "Миграции не применены (InvalidOperationException). Продолжаем работу...");
            }
            catch (Exception migrateEx)
            {
                // Все остальные ошибки миграций - логируем, но продолжаем
                logger.LogWarning(migrateEx, "Не удалось применить миграции. Продолжаем работу...");
            }

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
