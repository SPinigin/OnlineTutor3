using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            // Применяем миграции (создаст таблицы Identity, если их нет)
            // Важно: база данных OnlineTutor3 должна быть создана заранее через SQL-скрипт CreateDatabase.sql
            await context.Database.MigrateAsync();

            // Создаем роли
            await EnsureRolesCreatedAsync(roleManager);

            // Создаем администратора (если нужно)
            // await EnsureAdminCreatedAsync(userManager);
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

