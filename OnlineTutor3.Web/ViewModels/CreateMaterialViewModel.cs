using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для создания материала
    /// </summary>
    public class CreateMaterialViewModel
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        [Display(Name = "Название материала")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Выберите файл")]
        [Display(Name = "Файл")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Класс")]
        public int? ClassId { get; set; }

        [Display(Name = "Задание")]
        public int? AssignmentId { get; set; }

        [Display(Name = "Активный")]
        public bool IsActive { get; set; } = true;
    }
}

