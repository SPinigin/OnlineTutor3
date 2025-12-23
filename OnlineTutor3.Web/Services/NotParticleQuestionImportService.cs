using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Services
{
    /// <summary>
    /// Сервис для импорта вопросов теста на правописание частицы "не" из Excel
    /// </summary>
    public class NotParticleQuestionImportService
    {
        private readonly ILogger<NotParticleQuestionImportService> _logger;

        public NotParticleQuestionImportService(ILogger<NotParticleQuestionImportService> logger)
        {
            _logger = logger;
        }

        public async Task<List<ImportNotParticleQuestionRow>> ParseExcelFileAsync(IFormFile file)
        {
            var questions = new List<ImportNotParticleQuestionRow>();

            try
            {
                _logger.LogInformation("Начало парсинга файла импорта вопросов на правописание частицы \"не\". Файл: {FileName}, Размер: {FileSize} байт", 
                    file.FileName, file.Length);

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
                        var question = new ImportNotParticleQuestionRow { RowNumber = row };

                        question.TextWithGap = GetCellValue(worksheet, row, 1);
                        question.CorrectAnswer = GetCellValue(worksheet, row, 2);
                        question.FullText = GetCellValue(worksheet, row, 3);
                        question.Hint = GetCellValue(worksheet, row, 4);

                        // Пропускаем пустые строки
                        if (string.IsNullOrWhiteSpace(question.TextWithGap) &&
                            string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
                            string.IsNullOrWhiteSpace(question.FullText))
                        {
                            continue;
                        }

                        ValidateQuestion(question);
                        questions.Add(question);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "Ошибка обработки строки {RowNumber} в файле {FileName}", row, file.FileName);
                        var errorQuestion = new ImportNotParticleQuestionRow
                        {
                            RowNumber = row,
                            TextWithGap = GetCellValue(worksheet, row, 1) ?? "",
                            CorrectAnswer = GetCellValue(worksheet, row, 2) ?? "",
                            FullText = GetCellValue(worksheet, row, 3) ?? ""
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
            try
            {
                _logger.LogInformation("Начало генерации шаблона импорта вопросов на правописание частицы \"не\"");

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Вопросы");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Текст с (не)*";
                worksheet.Cells[1, 2].Value = "Правильный ответ*";
                worksheet.Cells[1, 3].Value = "Полный текст*";
                worksheet.Cells[1, 4].Value = "Подсказка";

                // Примеры данных
                worksheet.Cells[2, 1].Value = "Пережить (не) удачу";
                worksheet.Cells[2, 2].Value = "слитно";
                worksheet.Cells[2, 3].Value = "Пережить неудачу";
                worksheet.Cells[2, 4].Value = "Частица \"не\" с существительными пишется слитно, если слово приобретает противоположное значение.";

                worksheet.Cells[3, 1].Value = "(не) петух, а орёл";
                worksheet.Cells[3, 2].Value = "раздельно";
                worksheet.Cells[3, 3].Value = "не петух, а орёл";
                worksheet.Cells[3, 4].Value = "Частица \"не\" пишется раздельно, если есть противопоставление с союзом \"а\".";

                worksheet.Cells[4, 1].Value = "(не) правда";
                worksheet.Cells[4, 2].Value = "слитно";
                worksheet.Cells[4, 3].Value = "неправда";
                worksheet.Cells[4, 4].Value = "Частица \"не\" с существительными пишется слитно, если слово можно заменить синонимом без \"не\".";

                // Форматирование заголовков
                for (int i = 1; i <= 4; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                }

                // Настройка ширины столбцов
                worksheet.Column(1).Width = 30; // Текст с (не)
                worksheet.Column(2).Width = 20; // Правильный ответ
                worksheet.Column(3).Width = 30; // Полный текст
                worksheet.Column(4).Width = 60; // Подсказка

                // Добавляем лист с инструкциями
                var instructionSheet = package.Workbook.Worksheets.Add("Инструкция");
                instructionSheet.Cells[1, 1].Value = "Инструкция по заполнению вопросов";
                instructionSheet.Cells[1, 1].Style.Font.Bold = true;
                instructionSheet.Cells[1, 1].Style.Font.Size = 14;

                instructionSheet.Cells[3, 1].Value = "Формат заполнения:";
                instructionSheet.Cells[3, 1].Style.Font.Bold = true;
                instructionSheet.Cells[4, 1].Value = "1. Текст с (не) - используйте (не) вместо частицы \"не\" в тексте";
                instructionSheet.Cells[5, 1].Value = "2. Правильный ответ - укажите \"слитно\" или \"раздельно\"";
                instructionSheet.Cells[6, 1].Value = "3. Полный текст - текст с правильным написанием (слитно или раздельно)";
                instructionSheet.Cells[7, 1].Value = "4. Подсказка - объяснение правила (необязательно)";

                instructionSheet.Cells[9, 1].Value = "Примеры:";
                instructionSheet.Cells[9, 1].Style.Font.Bold = true;
                instructionSheet.Cells[10, 1].Value = "• Пережить (не) удачу | слитно | Пережить неудачу | Частица \"не\" с существительными пишется слитно";
                instructionSheet.Cells[11, 1].Value = "• (не) петух, а орёл | раздельно | не петух, а орёл | Есть противопоставление с союзом \"а\"";
                instructionSheet.Cells[12, 1].Value = "• (не) правда | слитно | неправда | Можно заменить синонимом \"ложь\"";

                instructionSheet.Cells[14, 1].Value = "Важно:";
                instructionSheet.Cells[14, 1].Style.Font.Bold = true;
                instructionSheet.Cells[15, 1].Value = "• Не удаляйте строку с заголовками";
                instructionSheet.Cells[16, 1].Value = "• Заполняйте данные начиная со 2-й строки";
                instructionSheet.Cells[17, 1].Value = "• Для правильного ответа используйте только: \"слитно\" или \"раздельно\"";
                instructionSheet.Cells[18, 1].Value = "• Используйте (не) вместо частицы \"не\" в тексте вопроса";

                instructionSheet.Cells.AutoFitColumns();

                var templateBytes = package.GetAsByteArray();
                _logger.LogInformation("Шаблон импорта вопросов на правописание частицы \"не\" успешно создан. Размер: {Size} байт", templateBytes.Length);
                return await Task.FromResult(templateBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации шаблона импорта вопросов на правописание частицы \"не\"");
                throw new InvalidOperationException($"Ошибка при генерации шаблона: {ex.Message}", ex);
            }
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

        private void ValidateQuestion(ImportNotParticleQuestionRow question)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(question.TextWithGap))
            {
                question.Errors.Add("Текст с (не) обязателен");
            }
            else if (!question.TextWithGap.Contains("(не)"))
            {
                question.Errors.Add("Текст должен содержать (не)");
            }

            if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
            {
                question.Errors.Add("Правильный ответ обязателен");
            }
            else
            {
                var normalized = question.CorrectAnswer.Trim().ToLower();
                if (normalized != "слитно" && normalized != "раздельно")
                {
                    question.Errors.Add("Правильный ответ должен быть 'слитно' или 'раздельно'");
                }
            }

            if (string.IsNullOrWhiteSpace(question.FullText))
            {
                question.Errors.Add("Полный текст обязателен");
            }
        }
    }
}

