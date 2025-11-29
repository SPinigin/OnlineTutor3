using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class OrthoeopyQuestionImportViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        public int OrthoeopyTestId { get; set; }

        [Required(ErrorMessage = "Выберите файл для импорта")]
        [Display(Name = "Excel файл с вопросами")]
        public IFormFile ExcelFile { get; set; } = null!;

        [Display(Name = "Баллы за каждый правильный ответ")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int PointsPerQuestion { get; set; } = 1;
    }

    public class ImportOrthoeopyQuestionRow
    {
        public int RowNumber { get; set; }
        public string Word { get; set; } = string.Empty;
        public int StressPosition { get; set; }
        public string WordWithStress { get; set; } = string.Empty;
        public string? Hint { get; set; }
        public string? WrongStressPositions { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();
    }
}

