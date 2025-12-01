using NLog;
using NLog.Web;
using OfficeOpenXml;
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
    builder.Services.AddMemoryCache(); // Добавляем кэширование в память
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddWeb();

    // Настройка EPPlus лицензии
    try
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        logger.Info("EPPlus license configured successfully");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "EPPlus license configuration failed");
    }

// Регистрация сервисов импорта вопросов
builder.Services.AddScoped<OnlineTutor3.Web.Services.SpellingQuestionImportService>();
builder.Services.AddScoped<OnlineTutor3.Web.Services.PunctuationQuestionImportService>();
builder.Services.AddScoped<OnlineTutor3.Web.Services.OrthoeopyQuestionImportService>();
builder.Services.AddScoped<OnlineTutor3.Web.Services.RegularQuestionImportService>();

    logger.Info("Building application. Environment: {Environment}", builder.Environment.EnvironmentName);
    
    var app = builder.Build();

    // Инициализация базы данных
    // ВАЖНО: Не прерываем запуск приложения, если инициализация БД не удалась
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            await DbInitializer.Initialize(scope.ServiceProvider);
        }
    }
    catch (Exception dbInitEx)
    {
        logger.Error(dbInitEx, "Ошибка при инициализации базы данных. Приложение продолжит работу, но некоторые функции могут быть недоступны.");
        // Не бросаем исключение, чтобы приложение могло запуститься
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

