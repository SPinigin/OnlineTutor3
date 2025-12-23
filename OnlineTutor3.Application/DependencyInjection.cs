using Microsoft.Extensions.DependencyInjection;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Application.Services;

namespace OnlineTutor3.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<ISpellingTestService, SpellingTestService>();
            services.AddScoped<IPunctuationTestService, PunctuationTestService>();
            services.AddScoped<IOrthoeopyTestService, OrthoeopyTestService>();
            services.AddScoped<IRegularTestService, RegularTestService>();
            services.AddScoped<INotParticleTestService, NotParticleTestService>();
            services.AddScoped<ITestAccessService, TestAccessService>();
            services.AddScoped<ITestResultService, TestResultService>();
            services.AddScoped<IAnswerService, AnswerService>();
            services.AddScoped<ITestEvaluationService, TestEvaluationService>();
            services.AddScoped<IStudentTestService, StudentTestService>();
            services.AddScoped<IStudentStatisticsService, StudentStatisticsService>();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddScoped<SecurityValidationService>();
            
            return services;
        }
    }
}

