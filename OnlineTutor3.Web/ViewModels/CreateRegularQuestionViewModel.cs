using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateRegularQuestionViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        [Display(Name = "Тест")]
        public int RegularTestId { get; set; }

        [Required(ErrorMessage = "Порядковый номер обязателен")]
        [Range(1, 1000, ErrorMessage = "Порядковый номер должен быть от 1 до 1000")]
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Баллы обязательны")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Текст вопроса обязателен")]
        [StringLength(1000, ErrorMessage = "Текст вопроса не может превышать 1000 символов")]
        [Display(Name = "Текст вопроса")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип вопроса обязателен")]
        [Display(Name = "Тип вопроса")]
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;

        [StringLength(1000, ErrorMessage = "Объяснение не может превышать 1000 символов")]
        [Display(Name = "Объяснение правильного ответа")]
        public string? Explanation { get; set; }

        [StringLength(500, ErrorMessage = "Подсказка не может превышать 500 символов")]
        [Display(Name = "Подсказка")]
        public string? Hint { get; set; }

        [Display(Name = "Варианты ответов")]
        public List<QuestionOptionViewModel> Options { get; set; } = new();
    }

    public class QuestionOptionViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Текст варианта обязателен")]
        [StringLength(500, ErrorMessage = "Текст варианта не может превышать 500 символов")]
        [Display(Name = "Текст варианта")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Правильный ответ")]
        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }
    }
}

