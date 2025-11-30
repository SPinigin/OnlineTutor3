using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class EditCalendarEventViewModel : CreateCalendarEventViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Занятие завершено")]
        public bool IsCompleted { get; set; }

        [StringLength(500)]
        [Display(Name = "Заметки")]
        public string? Notes { get; set; }
    }
}

