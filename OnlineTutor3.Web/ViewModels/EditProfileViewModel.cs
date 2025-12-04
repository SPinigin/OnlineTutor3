using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Web.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Неверный формат номера телефона")]
        [Display(Name = "Номер телефона")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Дата рождения обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        public DateTime DateOfBirth { get; set; }
    }
}

