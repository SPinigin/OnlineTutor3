using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Services
{
    /// <summary>
    /// Сервис для импорта вопросов по орфоэпии из Excel
    /// </summary>
    public class OrthoeopyQuestionImportService
    {
        private readonly ILogger<OrthoeopyQuestionImportService> _logger;

        public OrthoeopyQuestionImportService(ILogger<OrthoeopyQuestionImportService> logger)
        {
            _logger = logger;
        }

        public async Task<List<ImportOrthoeopyQuestionRow>> ParseExcelFileAsync(IFormFile file)
        {
            var questions = new List<ImportOrthoeopyQuestionRow>();

            try
            {
                _logger.LogInformation("Начало парсинга файла импорта вопросов по орфоэпии. Файл: {FileName}, Размер: {FileSize} байт",
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
                        var question = new ImportOrthoeopyQuestionRow { RowNumber = row };

                        question.Word = GetCellValue(worksheet, row, 1) ?? "";
                        var stressPositionStr = GetCellValue(worksheet, row, 2);
                        if (int.TryParse(stressPositionStr, out int stressPos))
                        {
                            question.StressPosition = stressPos;
                        }
                        question.WordWithStress = GetCellValue(worksheet, row, 3) ?? "";
                        question.Hint = GetCellValue(worksheet, row, 4);
                        question.WrongStressPositions = GetCellValue(worksheet, row, 5);

                        // Пропускаем пустые строки
                        if (string.IsNullOrWhiteSpace(question.Word) &&
                            string.IsNullOrWhiteSpace(question.WordWithStress))
                        {
                            continue;
                        }

                        ValidateQuestion(question);
                        questions.Add(question);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "Ошибка обработки строки {RowNumber} в файле {FileName}", row, file.FileName);
                        var errorQuestion = new ImportOrthoeopyQuestionRow
                        {
                            RowNumber = row,
                            Word = GetCellValue(worksheet, row, 1) ?? "",
                            WordWithStress = GetCellValue(worksheet, row, 3) ?? ""
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
                _logger.LogInformation("Начало генерации шаблона импорта вопросов по орфоэпии");

                using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Вопросы");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Слово*";
            worksheet.Cells[1, 2].Value = "Позиция ударения*";
            worksheet.Cells[1, 3].Value = "Слово с ударением*";
            worksheet.Cells[1, 4].Value = "Подсказка";
            worksheet.Cells[1, 5].Value = "Неправильные позиции (JSON)";

            // Примеры данных
            worksheet.Cells[2, 1].Value = "каталог";
            worksheet.Cells[2, 2].Value = "3";
            worksheet.Cells[2, 3].Value = "каталОг";
            worksheet.Cells[2, 4].Value = "Ударение падает на третий слог";
            worksheet.Cells[2, 5].Value = "[1,2]";

            worksheet.Cells[3, 1].Value = "звонит";
            worksheet.Cells[3, 2].Value = "2";
            worksheet.Cells[3, 3].Value = "звОнит";
            worksheet.Cells[3, 4].Value = "Ударение падает на второй слог";
            worksheet.Cells[3, 5].Value = "[1,3]";

            // Форматирование заголовков
            for (int i = 1; i <= 5; i++)
            {
                worksheet.Cells[1, i].Style.Font.Bold = true;
                // Убираем цветовое оформление, чтобы избежать проблем с SetColor
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 50;
            worksheet.Column(5).Width = 30;

            // Добавляем лист с инструкциями
            var instructionSheet = package.Workbook.Worksheets.Add("Инструкция");
            instructionSheet.Cells[1, 1].Value = "Инструкция по заполнению вопросов";
            instructionSheet.Cells[1, 1].Style.Font.Bold = true;
            instructionSheet.Cells[1, 1].Style.Font.Size = 14;

            instructionSheet.Cells[3, 1].Value = "Формат заполнения:";
            instructionSheet.Cells[3, 1].Style.Font.Bold = true;
            instructionSheet.Cells[4, 1].Value = "1. Слово - слово без ударения";
            instructionSheet.Cells[5, 1].Value = "2. Позиция ударения - номер слога с ударением (начиная с 1)";
            instructionSheet.Cells[6, 1].Value = "3. Слово с ударением - слово с обозначенным ударением (заглавной буквой)";
            instructionSheet.Cells[7, 1].Value = "4. Подсказка - объяснение правила (необязательно)";
            instructionSheet.Cells[8, 1].Value = "5. Неправильные позиции - JSON массив позиций (необязательно, например: [1,2])";

            instructionSheet.Cells[10, 1].Value = "Примеры:";
            instructionSheet.Cells[10, 1].Style.Font.Bold = true;
            instructionSheet.Cells[11, 1].Value = "• каталог | 3 | каталОг | Ударение падает на третий слог | [1,2]";
            instructionSheet.Cells[12, 1].Value = "• звонит | 2 | звОнит | Ударение падает на второй слог | [1,3]";

            instructionSheet.Cells[14, 1].Value = "Важно:";
            instructionSheet.Cells[14, 1].Style.Font.Bold = true;
            instructionSheet.Cells[15, 1].Value = "• Не удаляйте строку с заголовками";
            instructionSheet.Cells[16, 1].Value = "• Заполняйте данные начиная со 2-й строки";
            instructionSheet.Cells[17, 1].Value = "• Позиция ударения должна быть числом от 1 до 20";

            instructionSheet.Cells.AutoFitColumns();

                var templateBytes = package.GetAsByteArray();
                _logger.LogInformation("Шаблон импорта вопросов по орфоэпии успешно создан. Размер: {Size} байт", templateBytes.Length);
                return await Task.FromResult(templateBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации шаблона импорта вопросов по орфоэпии");
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

        private void ValidateQuestion(ImportOrthoeopyQuestionRow question)
        {
            if (string.IsNullOrWhiteSpace(question.Word))
            {
                question.Errors.Add("Слово обязательно");
            }

            if (question.StressPosition < 1 || question.StressPosition > 20)
            {
                question.Errors.Add("Позиция ударения должна быть от 1 до 20");
            }

            if (string.IsNullOrWhiteSpace(question.WordWithStress))
            {
                question.Errors.Add("Слово с ударением обязательно");
            }
        }
    }
}

