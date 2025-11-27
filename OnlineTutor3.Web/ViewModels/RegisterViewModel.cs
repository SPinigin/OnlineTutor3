using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50)]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата рождения обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен")]
        [Phone(ErrorMessage = "Неверный формат номера телефона")]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите роль")]
        [Display(Name = "Я регистрируюсь как")]
        public string Role { get; set; } = string.Empty;

        // Поля для студентов
        [Display(Name = "Школа")]
        [StringLength(200)]
        public string? School { get; set; }

        [Display(Name = "Класс")]
        [Range(1, 11, ErrorMessage = "Класс должен быть от 1 до 11")]
        public int? Grade { get; set; }

        // Поля для учителей
        [Display(Name = "Предметы преподавания")]
        public List<int>? SelectedSubjectIds { get; set; }

        [Display(Name = "Образование")]
        [StringLength(500)]
        public string? Education { get; set; }

        [Display(Name = "Опыт работы (лет)")]
        [Range(0, 50, ErrorMessage = "Опыт работы должен быть от 0 до 50 лет")]
        public int? Experience { get; set; }
    }
}

