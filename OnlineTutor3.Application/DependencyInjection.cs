using Microsoft.Extensions.DependencyInjection;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Application.Services;

namespace OnlineTutor3.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Регистрация сервисов
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<ISpellingTestService, SpellingTestService>();
            services.AddScoped<IPunctuationTestService, PunctuationTestService>();
            services.AddScoped<IOrthoeopyTestService, OrthoeopyTestService>();
            services.AddScoped<IRegularTestService, RegularTestService>();
            
            // Студентские сервисы
            services.AddScoped<ITestAccessService, TestAccessService>();
            services.AddScoped<ITestResultService, TestResultService>();
            services.AddScoped<IAnswerService, AnswerService>();
            services.AddScoped<ITestEvaluationService, TestEvaluationService>();
            services.AddScoped<IStudentTestService, StudentTestService>();
            
            return services;
        }
    }
}

