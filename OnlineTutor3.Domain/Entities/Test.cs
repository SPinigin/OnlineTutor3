using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Базовый класс для всех типов тестов
    /// </summary>
    public abstract class Test
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [Range(5, 300)]
        public int TimeLimit { get; set; } = 30; // в минутах

        [Range(1, 100)]
        public int MaxAttempts { get; set; } = 1;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool ShowHints { get; set; } = true;

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Тест по орфографии
    /// </summary>
    public class SpellingTest : Test
    {
        // Специфичные свойства для орфографии можно добавить позже
    }

    /// <summary>
    /// Тест по пунктуации
    /// </summary>
    public class PunctuationTest : Test
    {
        // Специфичные свойства для пунктуации можно добавить позже
    }

    /// <summary>
    /// Тест по орфоэпии
    /// </summary>
    public class OrthoeopyTest : Test
    {
        // Специфичные свойства для орфоэпии можно добавить позже
    }

    /// <summary>
    /// Классический тест (для всех предметов)
    /// </summary>
    public class RegularTest : Test
    {
        // Специфичные свойства для классических тестов можно добавить позже
    }

    /// <summary>
    /// Тест на правописание частицы "не" (слитно/раздельно)
    /// </summary>
    public class NotParticleTest : Test
    {
        // Специфичные свойства для теста на частицу "не" можно добавить позже
    }
}

