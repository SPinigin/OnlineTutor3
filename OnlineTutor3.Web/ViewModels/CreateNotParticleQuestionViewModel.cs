using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateNotParticleQuestionViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        [Display(Name = "Тест")]
        public int NotParticleTestId { get; set; }

        [Required(ErrorMessage = "Порядковый номер обязателен")]
        [Range(1, 1000, ErrorMessage = "Порядковый номер должен быть от 1 до 1000")]
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Баллы обязательны")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Текст с (не) обязателен")]
        [StringLength(500, ErrorMessage = "Текст не может превышать 500 символов")]
        [Display(Name = "Текст с (не)")]
        public string TextWithGap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Правильный ответ обязателен")]
        [StringLength(20, ErrorMessage = "Ответ не может превышать 20 символов")]
        [Display(Name = "Правильный ответ")]
        public string CorrectAnswer { get; set; } = string.Empty; // "слитно" или "раздельно"

        [Required(ErrorMessage = "Полный текст обязателен")]
        [StringLength(500, ErrorMessage = "Текст не может превышать 500 символов")]
        [Display(Name = "Полный текст")]
        public string FullText { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Подсказка не может превышать 500 символов")]
        [Display(Name = "Подсказка")]
        public string? Hint { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(CorrectAnswer))
            {
                var normalized = CorrectAnswer.Trim().ToLower();
                if (normalized != "слитно" && normalized != "раздельно")
                {
                    yield return new ValidationResult(
                        "Правильный ответ должен быть 'слитно' или 'раздельно'",
                        new[] { nameof(CorrectAnswer) });
                }
            }
        }
    }
}

