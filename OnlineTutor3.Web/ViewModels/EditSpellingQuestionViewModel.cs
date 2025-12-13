using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class EditSpellingQuestionViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Необходимо указать тест")]
        [Display(Name = "Тест")]
        public int SpellingTestId { get; set; }

        [Required(ErrorMessage = "Порядковый номер обязателен")]
        [Range(1, 1000, ErrorMessage = "Порядковый номер должен быть от 1 до 1000")]
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Баллы обязательны")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Слово с пропуском обязательно")]
        [StringLength(200, ErrorMessage = "Слово не может превышать 200 символов")]
        [Display(Name = "Слово с пропуском")]
        public string WordWithGap { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Буква не может превышать 10 символов")]
        [Display(Name = "Правильная буква")]
        public string? CorrectLetter { get; set; }

        [Required(ErrorMessage = "Полное слово обязательно")]
        [StringLength(200, ErrorMessage = "Слово не может превышать 200 символов")]
        [Display(Name = "Полное слово")]
        public string FullWord { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Подсказка не может превышать 500 символов")]
        [Display(Name = "Подсказка")]
        public string? Hint { get; set; }

        [Display(Name = "Требуется ответ")]
        public bool RequiresAnswer { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!RequiresAnswer)
            {
                CorrectLetter = null;
                yield break;
            }
            
            if (string.IsNullOrWhiteSpace(CorrectLetter))
            {
                yield return new ValidationResult(
                    "Правильная буква обязательна, когда требуется ответ",
                    new[] { nameof(CorrectLetter) });
            }
        }
    }
}

