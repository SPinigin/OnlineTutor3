using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Материал - файл, загруженный учителем для класса и/или задания
    /// </summary>
    public class Material
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FileName { get; set; }

        public long FileSize { get; set; }

        [StringLength(200)]
        public string? ContentType { get; set; }

        public MaterialType Type { get; set; }

        public int? ClassId { get; set; }

        public int? AssignmentId { get; set; }

        [Required]
        public string UploadedById { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Тип материала
    /// </summary>
    public enum MaterialType
    {
        Document = 1,      // Документы
        Video = 2,         // Видео
        Audio = 3,         // Аудио
        Image = 4,         // Изображения
        Presentation = 5,  // Презентации
        Other = 6          // Другое
    }
}

