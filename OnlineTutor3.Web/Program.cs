using NLog;
using NLog.Web;
using OfficeOpenXml;
using OnlineTutor3.Application;
using OnlineTutor3.Infrastructure;
using OnlineTutor3.Infrastructure.Data;
using OnlineTutor3.Web;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddMemoryCache();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddWeb();

    builder.Services.AddScoped<OnlineTutor3.Web.Services.SpellingQuestionImportService>();
    builder.Services.AddScoped<OnlineTutor3.Web.Services.PunctuationQuestionImportService>();
    builder.Services.AddScoped<OnlineTutor3.Web.Services.OrthoeopyQuestionImportService>();
    builder.Services.AddScoped<OnlineTutor3.Web.Services.RegularQuestionImportService>();

    var app = builder.Build();

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
    }
    app.UseWeb();

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

