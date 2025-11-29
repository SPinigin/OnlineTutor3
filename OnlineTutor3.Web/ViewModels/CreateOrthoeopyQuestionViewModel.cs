using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateOrthoeopyQuestionViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тест")]
        [Display(Name = "Тест")]
        public int OrthoeopyTestId { get; set; }

        [Required(ErrorMessage = "Порядковый номер обязателен")]
        [Range(1, 1000, ErrorMessage = "Порядковый номер должен быть от 1 до 1000")]
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Баллы обязательны")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        [Display(Name = "Баллы")]
        public int Points { get; set; } = 1;

        [Required(ErrorMessage = "Слово обязательно")]
        [StringLength(200, ErrorMessage = "Слово не может превышать 200 символов")]
        [Display(Name = "Слово")]
        public string Word { get; set; } = string.Empty;

        [Required(ErrorMessage = "Позиция ударения обязательна")]
        [Range(1, 20, ErrorMessage = "Позиция ударения должна быть от 1 до 20")]
        [Display(Name = "Позиция ударного слога")]
        public int StressPosition { get; set; } = 1;

        [Required(ErrorMessage = "Слово с ударением обязательно")]
        [StringLength(200, ErrorMessage = "Слово не может превышать 200 символов")]
        [Display(Name = "Слово с правильным ударением")]
        public string WordWithStress { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Неправильные варианты не могут превышать 100 символов")]
        [Display(Name = "Неправильные варианты (JSON массив позиций)")]
        public string? WrongStressPositions { get; set; }

        [StringLength(500, ErrorMessage = "Подсказка не может превышать 500 символов")]
        [Display(Name = "Подсказка")]
        public string? Hint { get; set; }
    }
}

