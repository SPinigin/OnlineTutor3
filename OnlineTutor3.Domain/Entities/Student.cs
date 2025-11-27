using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Модель ученика
    /// </summary>
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? ClassId { get; set; }

        [MaxLength(50)]
        public string? StudentNumber { get; set; }

        [StringLength(200)]
        public string? School { get; set; }

        public int? Grade { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

