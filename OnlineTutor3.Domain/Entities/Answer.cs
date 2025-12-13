using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Базовый класс для ответов студентов
    /// </summary>
    public abstract class Answer
    {
        public int Id { get; set; }

        [Required]
        public int TestResultId { get; set; }

        public bool IsCorrect { get; set; } = false;

        public int Points { get; set; } = 0;
    }

    /// <summary>
    /// Ответ на вопрос по орфографии
    /// </summary>
    public class SpellingAnswer : Answer
    {
        [Required]
        public int SpellingQuestionId { get; set; }

        [Required]
        [StringLength(10)]
        public string StudentAnswer { get; set; } = string.Empty; // Ответ студента (буква)

        /// <summary>
        /// Отмечен ли чекбокс "Буква не нужна"
        /// </summary>
        public bool NoLetterNeeded { get; set; } = false;
    }

    /// <summary>
    /// Ответ на вопрос по пунктуации
    /// </summary>
    public class PunctuationAnswer : Answer
    {
        [Required]
        public int PunctuationQuestionId { get; set; }

        [Required]
        [StringLength(50)]
        public string StudentAnswer { get; set; } = string.Empty; // Ответ студента (позиции знаков)
    }

    /// <summary>
    /// Ответ на вопрос по орфоэпии
    /// </summary>
    public class OrthoeopyAnswer : Answer
    {
        [Required]
        public int OrthoeopyQuestionId { get; set; }

        [Required]
        [Range(1, 20)]
        public int StudentAnswer { get; set; } // Ответ студента (позиция ударения)
    }

    /// <summary>
    /// Ответ на вопрос классического теста
    /// </summary>
    public class RegularAnswer : Answer
    {
        [Required]
        public int RegularQuestionId { get; set; }

        [StringLength(1000)]
        public string? StudentAnswer { get; set; } // Текстовый ответ (для TrueFalse)

        public int? SelectedOptionId { get; set; } // Выбранный вариант (для SingleChoice)
    }
}

