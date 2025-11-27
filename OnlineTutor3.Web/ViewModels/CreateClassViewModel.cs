using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Название онлайн-класса обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Display(Name = "Название онлайн-класса")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }
    }
}

