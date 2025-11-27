using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class EditAssignmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название задания обязательно")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        [Display(Name = "Название задания")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать предмет")]
        [Display(Name = "Предмет")]
        public int SubjectId { get; set; }

        [Display(Name = "Срок выполнения")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Активно")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Классы")]
        public List<int> SelectedClassIds { get; set; } = new List<int>();
    }
}

