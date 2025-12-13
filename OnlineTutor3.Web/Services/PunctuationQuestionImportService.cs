using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Services
{
    /// <summary>
    /// Сервис для импорта вопросов по пунктуации из Excel
    /// </summary>
    public class PunctuationQuestionImportService
    {
        private readonly ILogger<PunctuationQuestionImportService> _logger;

        public PunctuationQuestionImportService(ILogger<PunctuationQuestionImportService> logger)
        {
            _logger = logger;
        }

        private void ConfigureExcelPackage()
        {
            // Устанавливаем лицензию EPPlus (должна быть установлена в Program.cs, но на всякий случай устанавливаем здесь тоже)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _logger.LogDebug("EPPlus license context: {LicenseContext}", ExcelPackage.LicenseContext);
        }

        public async Task<List<ImportPunctuationQuestionRow>> ParseExcelFileAsync(IFormFile file)
        {
            var questions = new List<ImportPunctuationQuestionRow>();

            try
            {
                _logger.LogInformation("Начало парсинга файла импорта вопросов по пунктуации. Файл: {FileName}, Размер: {FileSize} байт",
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
                        var question = new ImportPunctuationQuestionRow { RowNumber = row };

                        question.SentenceWithNumbers = GetCellValue(worksheet, row, 1);
                        question.CorrectPositions = GetCellValue(worksheet, row, 2);
                        question.PlainSentence = GetCellValue(worksheet, row, 3);
                        question.Hint = GetCellValue(worksheet, row, 4);

                        // Пропускаем пустые строки
                        if (string.IsNullOrWhiteSpace(question.SentenceWithNumbers) &&
                            string.IsNullOrWhiteSpace(question.CorrectPositions))
                        {
                            continue;
                        }

                        ValidateQuestion(question);
                        questions.Add(question);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "Ошибка обработки строки {RowNumber} в файле {FileName}", row, file.FileName);
                        var errorQuestion = new ImportPunctuationQuestionRow
                        {
                            RowNumber = row,
                            SentenceWithNumbers = GetCellValue(worksheet, row, 1) ?? "",
                            CorrectPositions = GetCellValue(worksheet, row, 2) ?? ""
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
                _logger.LogInformation("Начало генерации шаблона импорта вопросов по пунктуации");
                ConfigureExcelPackage();

                using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Вопросы");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Предложение с номерами*";
            worksheet.Cells[1, 2].Value = "Правильные позиции*";
            worksheet.Cells[1, 3].Value = "Обычное предложение";
            worksheet.Cells[1, 4].Value = "Подсказка";

            // Примеры данных
            worksheet.Cells[2, 1].Value = "Пришла весна(1) и зацвели цветы(2)";
            worksheet.Cells[2, 2].Value = "1,2";
            worksheet.Cells[2, 3].Value = "Пришла весна, и зацвели цветы";
            worksheet.Cells[2, 4].Value = "Запятая ставится перед союзом \"и\" в сложном предложении";

            worksheet.Cells[3, 1].Value = "Он сказал(1) что придет(2)";
            worksheet.Cells[3, 2].Value = "1,2";
            worksheet.Cells[3, 3].Value = "Он сказал, что придет";
            worksheet.Cells[3, 4].Value = "Запятая ставится перед подчинительным союзом \"что\"";

            // Форматирование заголовков
            for (int i = 1; i <= 4; i++)
            {
                worksheet.Cells[1, i].Style.Font.Bold = true;
                // Убираем цветовое оформление, чтобы избежать проблем с SetColor
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 40;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 40;
            worksheet.Column(4).Width = 60;

            // Добавляем лист с инструкциями
            var instructionSheet = package.Workbook.Worksheets.Add("Инструкция");
            instructionSheet.Cells[1, 1].Value = "Инструкция по заполнению вопросов";
            instructionSheet.Cells[1, 1].Style.Font.Bold = true;
            instructionSheet.Cells[1, 1].Style.Font.Size = 14;

            instructionSheet.Cells[3, 1].Value = "Формат заполнения:";
            instructionSheet.Cells[3, 1].Style.Font.Bold = true;
            instructionSheet.Cells[4, 1].Value = "1. Предложение с номерами - предложение с пронумерованными позициями для знаков препинания";
            instructionSheet.Cells[5, 1].Value = "2. Правильные позиции - номера позиций через запятую (например: 1,2,3)";
            instructionSheet.Cells[6, 1].Value = "3. Обычное предложение - предложение с правильными знаками препинания (необязательно)";
            instructionSheet.Cells[7, 1].Value = "4. Подсказка - объяснение правила (необязательно)";

            instructionSheet.Cells[9, 1].Value = "Примеры:";
            instructionSheet.Cells[9, 1].Style.Font.Bold = true;
            instructionSheet.Cells[10, 1].Value = "• Пришла весна(1) и зацвели цветы(2) | 1,2 | Пришла весна, и зацвели цветы";
            instructionSheet.Cells[11, 1].Value = "• Он сказал(1) что придет(2) | 1,2 | Он сказал, что придет";

            instructionSheet.Cells[13, 1].Value = "Важно:";
            instructionSheet.Cells[13, 1].Style.Font.Bold = true;
            instructionSheet.Cells[14, 1].Value = "• Не удаляйте строку с заголовками";
            instructionSheet.Cells[15, 1].Value = "• Заполняйте данные начиная со 2-й строки";
            instructionSheet.Cells[16, 1].Value = "• Номера позиций указывайте через запятую без пробелов";

            instructionSheet.Cells.AutoFitColumns();

                var templateBytes = package.GetAsByteArray();
                _logger.LogInformation("Шаблон импорта вопросов по пунктуации успешно создан. Размер: {Size} байт", templateBytes.Length);
                return await Task.FromResult(templateBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации шаблона импорта вопросов по пунктуации");
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

        private void ValidateQuestion(ImportPunctuationQuestionRow question)
        {
            if (string.IsNullOrWhiteSpace(question.SentenceWithNumbers))
            {
                question.Errors.Add("Предложение с номерами обязательно");
            }

            if (string.IsNullOrWhiteSpace(question.CorrectPositions))
            {
                question.Errors.Add("Правильные позиции обязательны");
            }
            else
            {
                // Проверяем формат позиций
                var positions = question.CorrectPositions.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();

                if (positions.Length == 0)
                {
                    question.Errors.Add("Правильные позиции должны содержать хотя бы один номер");
                }
            }
        }
    }
}

