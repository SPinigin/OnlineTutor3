using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class PunctuationQuestionImportViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        public int PunctuationTestId { get; set; }

        [Required(ErrorMessage = "Выберите файл для импорта")]
        [Display(Name = "Excel файл с вопросами")]
        public IFormFile ExcelFile { get; set; } = null!;

        [Display(Name = "Баллы за каждый правильный ответ")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int PointsPerQuestion { get; set; } = 1;
    }

    public class ImportPunctuationQuestionRow
    {
        public int RowNumber { get; set; }
        public string? SentenceWithNumbers { get; set; }
        public string? CorrectPositions { get; set; }
        public string? PlainSentence { get; set; }
        public string? Hint { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();
    }
}

