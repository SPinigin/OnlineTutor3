using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class RegularQuestionImportViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        public int RegularTestId { get; set; }

        [Required(ErrorMessage = "Выберите файл для импорта")]
        [Display(Name = "Excel файл с вопросами")]
        public IFormFile ExcelFile { get; set; } = null!;

        [Display(Name = "Баллы за каждый правильный ответ")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int PointsPerQuestion { get; set; } = 1;
    }

    public class ImportRegularQuestionRow
    {
        public int RowNumber { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public string? Explanation { get; set; }
        public string? Hint { get; set; }
        public string? Options { get; set; } // JSON строка с вариантами ответов
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();
        
        // Вспомогательное свойство для десериализации вариантов ответов
        public List<QuestionOptionViewModel>? GetOptions()
        {
            if (string.IsNullOrWhiteSpace(Options))
                return null;
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<QuestionOptionViewModel>>(Options);
            }
            catch
            {
                return null;
            }
        }
    }
}

