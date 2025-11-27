using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateSpellingTestViewModel
    {
        [Required(ErrorMessage = "Название теста обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        [Display(Name = "Название теста")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать задание")]
        [Display(Name = "Задание")]
        public int AssignmentId { get; set; }

        [Range(5, 300, ErrorMessage = "Время должно быть от 5 до 300 минут")]
        [Display(Name = "Время на выполнение (минут)")]
        public int TimeLimit { get; set; } = 30;

        [Range(1, 100, ErrorMessage = "Количество попыток должно быть от 1 до 100")]
        [Display(Name = "Количество попыток")]
        public int MaxAttempts { get; set; } = 1;

        [Display(Name = "Дата начала")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Дата окончания")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Показывать подсказки")]
        public bool ShowHints { get; set; } = true;

        [Display(Name = "Показывать правильные ответы")]
        public bool ShowCorrectAnswers { get; set; } = true;

        [Display(Name = "Тест активен")]
        public bool IsActive { get; set; } = true;
    }
}

