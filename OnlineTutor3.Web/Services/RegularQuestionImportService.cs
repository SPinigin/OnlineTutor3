using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Services
{
    /// <summary>
    /// Сервис для импорта вопросов классических тестов из Excel
    /// </summary>
    public class RegularQuestionImportService
    {
        private readonly ILogger<RegularQuestionImportService> _logger;

        public RegularQuestionImportService(ILogger<RegularQuestionImportService> logger)
        {
            _logger = logger;
        }

        private void ConfigureExcelPackage()
        {
            try
            {
                if (ExcelPackage.LicenseContext == LicenseContext.Commercial)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                }
                _logger.LogDebug("EPPlus license context: {LicenseContext}", ExcelPackage.LicenseContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка настройки EPPlus license");
            }
        }

        public async Task<List<ImportRegularQuestionRow>> ParseExcelFileAsync(IFormFile file)
        {
            var questions = new List<ImportRegularQuestionRow>();

            try
            {
                _logger.LogInformation("Начало парсинга файла импорта классических вопросов. Файл: {FileName}, Размер: {FileSize} байт",
                    file.FileName, file.Length);
                ConfigureExcelPackage();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);

                if (package.Workbook?.Worksheets?.Count == 0)
                {
                    _logger.LogWarning("Excel файл не содержит листов. Файл: {FileName}", file.FileName);
                    throw new InvalidOperationException("Excel файл не содержит листов");
                }

                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet.Dimension == null)
                {
                    _logger.LogWarning("Excel файл пустой. Файл: {FileName}", file.FileName);
                    return questions;
                }

                var rowCount = worksheet.Dimension.Rows;
                _logger.LogDebug("Найдено {RowCount} строк в файле {FileName}", rowCount, file.FileName);

                if (rowCount < 2)
                {
                    _logger.LogWarning("Excel файл содержит только заголовки. Файл: {FileName}", file.FileName);
                    throw new InvalidOperationException("Excel файл должен содержать заголовки и хотя бы одну строку данных");
                }

                // Читаем данные начиная со 2-й строки
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var question = new ImportRegularQuestionRow { RowNumber = row };

                        question.Text = GetCellValue(worksheet, row, 1);
                        question.Type = GetCellValue(worksheet, row, 2);
                        
                        // Читаем варианты ответов (до 6 вариантов)
                        var options = new List<QuestionOptionViewModel>();
                        for (int optIndex = 0; optIndex < 6; optIndex++)
                        {
                            var optionText = GetCellValue(worksheet, row, 3 + optIndex * 2); // Столбцы 3, 5, 7, 9, 11, 13
                            var isCorrectStr = GetCellValue(worksheet, row, 4 + optIndex * 2); // Столбцы 4, 6, 8, 10, 12, 14
                            
                            if (!string.IsNullOrWhiteSpace(optionText))
                            {
                                var isCorrect = false;
                                if (!string.IsNullOrWhiteSpace(isCorrectStr))
                                {
                                    isCorrectStr = isCorrectStr.Trim().ToLowerInvariant();
                                    isCorrect = isCorrectStr == "да" || isCorrectStr == "yes" || 
                                                isCorrectStr == "1" || isCorrectStr == "true" || 
                                                isCorrectStr == "✓" || isCorrectStr == "+";
                                }
                                
                                options.Add(new QuestionOptionViewModel
                                {
                                    Text = optionText,
                                    IsCorrect = isCorrect,
                                    OrderIndex = optIndex + 1
                                });
                            }
                        }

                        question.Explanation = GetCellValue(worksheet, row, 15);
                        question.Hint = GetCellValue(worksheet, row, 16);

                        // Сохраняем варианты ответов как JSON
                        if (options.Any())
                        {
                            question.Options = System.Text.Json.JsonSerializer.Serialize(options);
                        }

                        // Пропускаем пустые строки
                        if (string.IsNullOrWhiteSpace(question.Text) && !options.Any())
                        {
                            continue;
                        }

                        ValidateQuestion(question, options);
                        questions.Add(question);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "Ошибка обработки строки {RowNumber} в файле {FileName}", row, file.FileName);
                        var errorQuestion = new ImportRegularQuestionRow
                        {
                            RowNumber = row,
                            Text = GetCellValue(worksheet, row, 1) ?? ""
                        };
                        errorQuestion.Errors.Add($"Ошибка обработки строки: {rowEx.Message}");
                        questions.Add(errorQuestion);
                    }
                }

                _logger.LogInformation("Парсинг завершен успешно. Всего вопросов: {Count}", questions.Count);
                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при чтении Excel файла");
                throw new InvalidOperationException($"Ошибка при чтении Excel файла: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GenerateTemplateAsync()
        {
            ConfigureExcelPackage();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Вопросы");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Текст вопроса*";
            worksheet.Cells[1, 2].Value = "Тип вопроса*";
            worksheet.Cells[1, 3].Value = "Вариант 1*";
            worksheet.Cells[1, 4].Value = "Правильный 1*";
            worksheet.Cells[1, 5].Value = "Вариант 2*";
            worksheet.Cells[1, 6].Value = "Правильный 2";
            worksheet.Cells[1, 7].Value = "Вариант 3";
            worksheet.Cells[1, 8].Value = "Правильный 3";
            worksheet.Cells[1, 9].Value = "Вариант 4";
            worksheet.Cells[1, 10].Value = "Правильный 4";
            worksheet.Cells[1, 11].Value = "Вариант 5";
            worksheet.Cells[1, 12].Value = "Правильный 5";
            worksheet.Cells[1, 13].Value = "Вариант 6";
            worksheet.Cells[1, 14].Value = "Правильный 6";
            worksheet.Cells[1, 15].Value = "Объяснение";
            worksheet.Cells[1, 16].Value = "Подсказка";

            // Примеры данных
            worksheet.Cells[2, 1].Value = "Столица России?";
            worksheet.Cells[2, 2].Value = "1";
            worksheet.Cells[2, 3].Value = "Москва";
            worksheet.Cells[2, 4].Value = "Да";
            worksheet.Cells[2, 5].Value = "Санкт-Петербург";
            worksheet.Cells[2, 6].Value = "";
            worksheet.Cells[2, 7].Value = "Казань";
            worksheet.Cells[2, 8].Value = "";
            worksheet.Cells[2, 9].Value = "Новосибирск";
            worksheet.Cells[2, 10].Value = "";
            worksheet.Cells[2, 15].Value = "Москва является столицей России с 1918 года";
            worksheet.Cells[2, 16].Value = "Вспомните главный город страны";

            worksheet.Cells[3, 1].Value = "Какие из перечисленных городов находятся в России?";
            worksheet.Cells[3, 2].Value = "2";
            worksheet.Cells[3, 3].Value = "Москва";
            worksheet.Cells[3, 4].Value = "Да";
            worksheet.Cells[3, 5].Value = "Париж";
            worksheet.Cells[3, 6].Value = "";
            worksheet.Cells[3, 7].Value = "Санкт-Петербург";
            worksheet.Cells[3, 8].Value = "Да";
            worksheet.Cells[3, 9].Value = "Лондон";
            worksheet.Cells[3, 10].Value = "";
            worksheet.Cells[3, 15].Value = "Москва и Санкт-Петербург - крупнейшие города России";

            // Форматирование заголовков
            for (int i = 1; i <= 16; i++)
            {
                worksheet.Cells[1, i].Style.Font.Bold = true;
                worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 40; // Текст вопроса
            worksheet.Column(2).Width = 15; // Тип вопроса
            for (int i = 3; i <= 14; i += 2)
            {
                worksheet.Column(i).Width = 25; // Варианты ответов
                worksheet.Column(i + 1).Width = 12; // Правильный
            }
            worksheet.Column(15).Width = 50; // Объяснение
            worksheet.Column(16).Width = 50; // Подсказка

            // Добавляем лист с инструкциями
            var instructionSheet = package.Workbook.Worksheets.Add("Инструкция");
            instructionSheet.Cells[1, 1].Value = "Инструкция по заполнению вопросов";
            instructionSheet.Cells[1, 1].Style.Font.Bold = true;
            instructionSheet.Cells[1, 1].Style.Font.Size = 14;

            instructionSheet.Cells[3, 1].Value = "Формат заполнения:";
            instructionSheet.Cells[3, 1].Style.Font.Bold = true;
            instructionSheet.Cells[4, 1].Value = "1. Текст вопроса - формулировка вопроса (обязательно)";
            instructionSheet.Cells[5, 1].Value = "2. Тип вопроса - 1 (Одиночный выбор), 2 (Множественный выбор), 3 (Верно/Неверно)";
            instructionSheet.Cells[6, 1].Value = "3. Варианты ответов - текст варианта ответа (минимум 2 варианта обязательно)";
            instructionSheet.Cells[7, 1].Value = "4. Правильный - Да/Нет или 1/0 для каждого варианта";
            instructionSheet.Cells[8, 1].Value = "5. Объяснение - объяснение правильного ответа (необязательно)";
            instructionSheet.Cells[9, 1].Value = "6. Подсказка - подсказка для ученика (необязательно)";

            instructionSheet.Cells[11, 1].Value = "Примеры типов вопросов:";
            instructionSheet.Cells[11, 1].Style.Font.Bold = true;
            instructionSheet.Cells[12, 1].Value = "• 1 - Одиночный выбор (только один правильный ответ)";
            instructionSheet.Cells[13, 1].Value = "• 2 - Множественный выбор (несколько правильных ответов)";
            instructionSheet.Cells[14, 1].Value = "• 3 - Верно/Неверно (только два варианта: Верно/Неверно)";

            instructionSheet.Cells[16, 1].Value = "Важно:";
            instructionSheet.Cells[16, 1].Style.Font.Bold = true;
            instructionSheet.Cells[17, 1].Value = "• Не удаляйте строку с заголовками";
            instructionSheet.Cells[18, 1].Value = "• Заполняйте данные начиная со 2-й строки";
            instructionSheet.Cells[19, 1].Value = "• Для одиночного выбора должен быть ровно один правильный ответ";
            instructionSheet.Cells[20, 1].Value = "• Для множественного выбора должно быть хотя бы два правильных ответа";
            instructionSheet.Cells[21, 1].Value = "• Для Верно/Неверно используйте только два варианта ответа";

            instructionSheet.Cells.AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }

        private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            try
            {
                var cell = worksheet.Cells[row, col];
                return cell?.Value?.ToString()?.Trim();
            }
            catch
            {
                return null;
            }
        }

        private void ValidateQuestion(ImportRegularQuestionRow question, List<QuestionOptionViewModel> options)
        {
            if (string.IsNullOrWhiteSpace(question.Text))
            {
                question.Errors.Add("Текст вопроса обязателен");
            }

            if (string.IsNullOrWhiteSpace(question.Type))
            {
                question.Errors.Add("Тип вопроса обязателен");
            }
            else
            {
                var typeStr = question.Type.Trim();
                if (typeStr != "1" && typeStr != "2" && typeStr != "3" &&
                    !typeStr.Equals("Одиночный выбор", StringComparison.OrdinalIgnoreCase) &&
                    !typeStr.Equals("Множественный выбор", StringComparison.OrdinalIgnoreCase) &&
                    !typeStr.Equals("Верно/Неверно", StringComparison.OrdinalIgnoreCase))
                {
                    question.Errors.Add("Тип вопроса должен быть: 1, 2, 3 или 'Одиночный выбор', 'Множественный выбор', 'Верно/Неверно'");
                }
            }

            if (options.Count < 2)
            {
                question.Errors.Add("Необходимо добавить хотя бы 2 варианта ответа");
            }

            if (options.Any())
            {
                var correctCount = options.Count(o => o.IsCorrect);
                var typeStr = question.Type?.Trim() ?? "";

                if (typeStr == "1" || typeStr.Equals("Одиночный выбор", StringComparison.OrdinalIgnoreCase))
                {
                    if (correctCount != 1)
                    {
                        question.Errors.Add("Для вопроса с одиночным выбором должен быть ровно один правильный ответ");
                    }
                }
                else if (typeStr == "2" || typeStr.Equals("Множественный выбор", StringComparison.OrdinalIgnoreCase))
                {
                    if (correctCount < 1)
                    {
                        question.Errors.Add("Для вопроса с множественным выбором должен быть хотя бы один правильный ответ");
                    }
                }
                else if (typeStr == "3" || typeStr.Equals("Верно/Неверно", StringComparison.OrdinalIgnoreCase))
                {
                    if (options.Count != 2)
                    {
                        question.Errors.Add("Для вопроса Верно/Неверно должно быть ровно 2 варианта ответа");
                    }
                    if (correctCount != 1)
                    {
                        question.Errors.Add("Для вопроса Верно/Неверно должен быть ровно один правильный ответ");
                    }
                }
            }
        }
    }
}

