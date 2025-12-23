using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class NotParticleQuestionImportViewModel
    {
        [Required]
        public int NotParticleTestId { get; set; }

        [Required(ErrorMessage = "Выберите файл для импорта")]
        [Display(Name = "Файл Excel (.xlsx, .xls)")]
        public IFormFile? ExcelFile { get; set; }

        [Required(ErrorMessage = "Укажите количество баллов")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы за вопрос")]
        public int PointsPerQuestion { get; set; } = 1;
    }

    public class ImportNotParticleQuestionRow
    {
        public int RowNumber { get; set; }
        public string? TextWithGap { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? FullText { get; set; }
        public string? Hint { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public bool IsValid => !Errors.Any();
    }
}

