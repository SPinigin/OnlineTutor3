using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateCalendarEventViewModel
    {
        [Required(ErrorMessage = "Название занятия обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        [Display(Name = "Название занятия")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Дата и время начала обязательны")]
        [Display(Name = "Начало занятия")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "Дата и время окончания обязательны")]
        [Display(Name = "Окончание занятия")]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        [Display(Name = "Класс")]
        public int? ClassId { get; set; }

        [Display(Name = "Ученик")]
        public int? StudentId { get; set; }

        [StringLength(200)]
        [Display(Name = "Место проведения")]
        public string? Location { get; set; }

        [Display(Name = "Цвет")]
        public string? Color { get; set; }

        [Display(Name = "Повторяющееся событие")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Повторять")]
        public string? RecurrencePattern { get; set; }
    }
}

