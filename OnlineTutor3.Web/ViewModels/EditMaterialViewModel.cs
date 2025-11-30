using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для редактирования материала
    /// </summary>
    public class EditMaterialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        [Display(Name = "Название материала")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Новый файл (оставьте пустым, чтобы не менять)")]
        public IFormFile? NewFile { get; set; }

        [Display(Name = "Класс")]
        public int? ClassId { get; set; }

        [Display(Name = "Задание")]
        public int? AssignmentId { get; set; }

        [Display(Name = "Активный")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Текущий файл")]
        public string? CurrentFileName { get; set; }
    }
}

