using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Связь многие-ко-многим между заданиями и классами
    /// </summary>
    public class AssignmentClass
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}

