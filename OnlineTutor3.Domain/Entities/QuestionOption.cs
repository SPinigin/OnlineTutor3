using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Вариант ответа для вопроса классического теста
    /// </summary>
    public class RegularQuestionOption
    {
        public int Id { get; set; }

        [Required]
        public int RegularQuestionId { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        public int OrderIndex { get; set; }
    }
}

