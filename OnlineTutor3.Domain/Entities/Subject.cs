using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Модель предмета преподавания
    /// </summary>
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int OrderIndex { get; set; }
    }
}

