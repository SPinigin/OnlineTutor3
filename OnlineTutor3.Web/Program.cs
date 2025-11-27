using NLog;
using NLog.Web;
using OnlineTutor3.Application;
using OnlineTutor3.Infrastructure;
using OnlineTutor3.Infrastructure.Data;
using OnlineTutor3.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Настройка логирования
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Настройка сервисов
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddWeb();

    logger.Info("Building application. Environment: {Environment}", builder.Environment.EnvironmentName);
    
    var app = builder.Build();

    // Инициализация базы данных
    using (var scope = app.Services.CreateScope())
    {
        await DbInitializer.Initialize(scope.ServiceProvider);
    }

    // Настройка middleware
    app.UseWeb();

    logger.Info("Приложение готово к запуску");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Приложение завершилось с ошибкой");
    throw;
}
finally
{
    LogManager.Shutdown();
}

