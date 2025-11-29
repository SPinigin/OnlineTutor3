using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class EditPunctuationQuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Необходимо указать тест")]
        [Display(Name = "Тест")]
        public int PunctuationTestId { get; set; }

        [Required(ErrorMessage = "Порядковый номер обязателен")]
        [Range(1, 1000, ErrorMessage = "Порядковый номер должен быть от 1 до 1000")]
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Баллы обязательны")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Предложение с номерами обязательно")]
        [StringLength(1000, ErrorMessage = "Предложение не может превышать 1000 символов")]
        [Display(Name = "Предложение с номерами")]
        public string SentenceWithNumbers { get; set; } = string.Empty;

        [Required(ErrorMessage = "Правильные позиции обязательны")]
        [StringLength(50, ErrorMessage = "Позиции не могут превышать 50 символов")]
        [Display(Name = "Правильные позиции знаков")]
        public string CorrectPositions { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Предложение не может превышать 1000 символов")]
        [Display(Name = "Обычное предложение")]
        public string? PlainSentence { get; set; }

        [StringLength(500, ErrorMessage = "Подсказка не может превышать 500 символов")]
        [Display(Name = "Подсказка")]
        public string? Hint { get; set; }
    }
}

