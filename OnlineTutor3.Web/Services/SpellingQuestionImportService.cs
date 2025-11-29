using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Services
{
    /// <summary>
    /// Сервис для импорта вопросов по орфографии из Excel
    /// </summary>
    public class SpellingQuestionImportService
    {
        private readonly ILogger<SpellingQuestionImportService> _logger;

        public SpellingQuestionImportService(ILogger<SpellingQuestionImportService> logger)
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

        public async Task<List<ImportSpellingQuestionRow>> ParseExcelFileAsync(IFormFile file)
        {
            var questions = new List<ImportSpellingQuestionRow>();

            try
            {
                _logger.LogInformation("Начало парсинга файла импорта вопросов по орфографии. Файл: {FileName}, Размер: {FileSize} байт", 
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
                        var question = new ImportSpellingQuestionRow { RowNumber = row };

                        question.WordWithGap = GetCellValue(worksheet, row, 1);
                        question.CorrectLetter = GetCellValue(worksheet, row, 2);
                        question.FullWord = GetCellValue(worksheet, row, 3);
                        question.Hint = GetCellValue(worksheet, row, 4);

                        // Пропускаем пустые строки
                        if (string.IsNullOrWhiteSpace(question.WordWithGap) &&
                            string.IsNullOrWhiteSpace(question.CorrectLetter) &&
                            string.IsNullOrWhiteSpace(question.FullWord))
                        {
                            continue;
                        }

                        ValidateQuestion(question);
                        questions.Add(question);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "Ошибка обработки строки {RowNumber} в файле {FileName}", row, file.FileName);
                        var errorQuestion = new ImportSpellingQuestionRow
                        {
                            RowNumber = row,
                            WordWithGap = GetCellValue(worksheet, row, 1) ?? "",
                            CorrectLetter = GetCellValue(worksheet, row, 2) ?? "",
                            FullWord = GetCellValue(worksheet, row, 3) ?? ""
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
            worksheet.Cells[1, 1].Value = "Слово (с пропуском)*";
            worksheet.Cells[1, 2].Value = "Правильная буква*";
            worksheet.Cells[1, 3].Value = "Полное слово*";
            worksheet.Cells[1, 4].Value = "Подсказка";

            // Примеры данных (используем нижнее подчеркивание вместо многоточия)
            worksheet.Cells[2, 1].Value = "Прол_тает";
            worksheet.Cells[2, 2].Value = "е";
            worksheet.Cells[2, 3].Value = "пролетает";
            worksheet.Cells[2, 4].Value = "Безударная гласная \"е\" в корне слова \"пролетает\" проверяется подбором проверочного слова, где эта гласная находится под ударением.";

            worksheet.Cells[3, 1].Value = "пож_лтели";
            worksheet.Cells[3, 2].Value = "е";
            worksheet.Cells[3, 3].Value = "пожелтели";
            worksheet.Cells[3, 4].Value = "Безударная гласная \"е\" в корне слова \"пожелтели\" проверяется подбором проверочного слова \"жёлтый\".";

            worksheet.Cells[4, 1].Value = "сн_говик";
            worksheet.Cells[4, 2].Value = "е";
            worksheet.Cells[4, 3].Value = "снеговик";
            worksheet.Cells[4, 4].Value = "Безударная гласная \"е\" в корне слова \"снеговик\" проверяется с помощью слова \"снег\".";

            // Форматирование заголовков
            for (int i = 1; i <= 4; i++)
            {
                worksheet.Cells[1, i].Style.Font.Bold = true;
                worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 25; // Слово с пропуском
            worksheet.Column(2).Width = 18; // Правильная буква
            worksheet.Column(3).Width = 25; // Полное слово
            worksheet.Column(4).Width = 60; // Подсказка

            // Добавляем лист с инструкциями
            var instructionSheet = package.Workbook.Worksheets.Add("Инструкция");
            instructionSheet.Cells[1, 1].Value = "Инструкция по заполнению вопросов";
            instructionSheet.Cells[1, 1].Style.Font.Bold = true;
            instructionSheet.Cells[1, 1].Style.Font.Size = 14;

            instructionSheet.Cells[3, 1].Value = "Формат заполнения:";
            instructionSheet.Cells[3, 1].Style.Font.Bold = true;
            instructionSheet.Cells[4, 1].Value = "1. Слово с пропуском - используйте символ _ (нижнее подчеркивание) для обозначения пропущенной буквы";
            instructionSheet.Cells[5, 1].Value = "2. Правильная буква - одна или несколько букв (а, е, и, о, у, я, ё). Для нескольких пропусков используйте запятую: а,о";
            instructionSheet.Cells[6, 1].Value = "3. Полное слово - слово без пропусков";
            instructionSheet.Cells[7, 1].Value = "4. Подсказка - объяснение правила (необязательно)";

            instructionSheet.Cells[9, 1].Value = "Примеры:";
            instructionSheet.Cells[9, 1].Style.Font.Bold = true;
            instructionSheet.Cells[10, 1].Value = "• д_ждливый | о | дождливый | Проверочное слово: дождь";
            instructionSheet.Cells[11, 1].Value = "• л_сник | е | лесник | Проверочное слово: лес";
            instructionSheet.Cells[12, 1].Value = "• ст_р_жил | о,о | сторожил | Проверочное слово: сторож";

            instructionSheet.Cells[14, 1].Value = "Важно:";
            instructionSheet.Cells[14, 1].Style.Font.Bold = true;
            instructionSheet.Cells[15, 1].Value = "• Не удаляйте строку с заголовками";
            instructionSheet.Cells[16, 1].Value = "• Заполняйте данные начиная со 2-й строки";
            instructionSheet.Cells[17, 1].Value = "• Для нескольких пропусков используйте запятую: а,о";
            instructionSheet.Cells[18, 1].Value = "• Используйте символ _ (нижнее подчеркивание) для обозначения пропущенной буквы";

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

        private void ValidateQuestion(ImportSpellingQuestionRow question)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(question.WordWithGap))
            {
                question.Errors.Add("Слово с пропуском обязательно");
            }
            else if (!question.WordWithGap.Contains("_"))
            {
                question.Errors.Add("Слово должно содержать символ пропуска (_)");
            }

            if (string.IsNullOrWhiteSpace(question.CorrectLetter))
            {
                question.Errors.Add("Правильная буква обязательна");
            }

            if (string.IsNullOrWhiteSpace(question.FullWord))
            {
                question.Errors.Add("Полное слово обязательно");
            }

            // Проверка соответствия
            if (!string.IsNullOrWhiteSpace(question.WordWithGap) &&
                !string.IsNullOrWhiteSpace(question.FullWord) &&
                !string.IsNullOrWhiteSpace(question.CorrectLetter))
            {
                // Подсчитываем количество пропусков в слове
                int gapCount = question.WordWithGap.Count(c => c == '_');

                // Разбиваем правильные буквы по запятой
                var correctLetters = question.CorrectLetter.Split(',')
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToArray();

                // Проверяем, что количество букв соответствует количеству пропусков
                if (correctLetters.Length != gapCount)
                {
                    question.Errors.Add($"Количество букв ({correctLetters.Length}) не соответствует количеству пропусков ({gapCount})");
                    return;
                }

                // Заменяем пропуски по очереди
                var reconstructed = question.WordWithGap;
                foreach (var letter in correctLetters)
                {
                    int index = reconstructed.IndexOf('_');
                    if (index >= 0)
                    {
                        reconstructed = reconstructed.Remove(index, 1).Insert(index, letter);
                    }
                }

                // Сравниваем результат с полным словом
                if (!reconstructed.Equals(question.FullWord, StringComparison.OrdinalIgnoreCase))
                {
                    question.Errors.Add("Полное слово не соответствует слову с заменёнными буквами");
                }
            }
        }
    }
}

