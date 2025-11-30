using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;
using OnlineTutor3.Infrastructure.Repositories;

namespace OnlineTutor3.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Регистрация DatabaseConnection
            services.AddScoped<IDatabaseConnection>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = LogManager.GetLogger(typeof(DatabaseConnection).FullName ?? nameof(DatabaseConnection));
                return new DatabaseConnection(config, logger);
            });

            // Регистрация репозиториев
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<ITeacherSubjectRepository, TeacherSubjectRepository>();
            services.AddScoped<IAssignmentRepository, AssignmentRepository>();
            services.AddScoped<IAssignmentClassRepository, AssignmentClassRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
            services.AddScoped<ISpellingTestRepository, SpellingTestRepository>();
            services.AddScoped<IPunctuationTestRepository, PunctuationTestRepository>();
            services.AddScoped<IOrthoeopyTestRepository, OrthoeopyTestRepository>();
            services.AddScoped<IRegularTestRepository, RegularTestRepository>();
            services.AddScoped<ISpellingQuestionRepository, SpellingQuestionRepository>();
            services.AddScoped<IPunctuationQuestionRepository, PunctuationQuestionRepository>();
            services.AddScoped<IOrthoeopyQuestionRepository, OrthoeopyQuestionRepository>();
            services.AddScoped<IRegularQuestionRepository, RegularQuestionRepository>();
            services.AddScoped<IRegularQuestionOptionRepository, RegularQuestionOptionRepository>();
            services.AddScoped<ISpellingTestResultRepository, SpellingTestResultRepository>();
            services.AddScoped<IPunctuationTestResultRepository, PunctuationTestResultRepository>();
            services.AddScoped<IOrthoeopyTestResultRepository, OrthoeopyTestResultRepository>();
            services.AddScoped<IRegularTestResultRepository, RegularTestResultRepository>();
            services.AddScoped<ISpellingAnswerRepository, SpellingAnswerRepository>();
            services.AddScoped<IPunctuationAnswerRepository, PunctuationAnswerRepository>();
            services.AddScoped<IOrthoeopyAnswerRepository, OrthoeopyAnswerRepository>();
            services.AddScoped<IRegularAnswerRepository, RegularAnswerRepository>();

            // Настройка Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Настройки пароля
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // Настройки пользователя
                options.User.RequireUniqueEmail = true;

                // Настройки подтверждения email
                options.SignIn.RequireConfirmedEmail = true;

                // Настройки блокировки
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ApplicationDbContext для Identity
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Настройка cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}

