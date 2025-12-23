using System.ComponentModel.DataAnnotations;

namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Базовый класс для вопросов
    /// </summary>
    public abstract class Question
    {
        public int Id { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        [Range(1, 100)]
        public int Points { get; set; } = 1;

        [StringLength(500)]
        public string? Hint { get; set; }
    }

    /// <summary>
    /// Вопрос для теста по орфографии
    /// </summary>
    public class SpellingQuestion : Question
    {
        [Required]
        public int SpellingTestId { get; set; }

        [Required]
        [StringLength(200)]
        public string WordWithGap { get; set; } = string.Empty; // Слово с пропуском

        [StringLength(10)]
        public string? CorrectLetter { get; set; } // Правильная буква (необязательна, если RequiresAnswer = false)

        [Required]
        [StringLength(200)]
        public string FullWord { get; set; } = string.Empty; // Полное слово

        /// <summary>
        /// Требуется ли ответ на этот вопрос. Если false, то пустой ответ считается правильным.
        /// </summary>
        public bool RequiresAnswer { get; set; } = true;
    }

    /// <summary>
    /// Вопрос для теста по пунктуации
    /// </summary>
    public class PunctuationQuestion : Question
    {
        [Required]
        public int PunctuationTestId { get; set; }

        [Required]
        [StringLength(1000)]
        public string SentenceWithNumbers { get; set; } = string.Empty; // Предложение с номерами позиций

        [Required]
        [StringLength(50)]
        public string CorrectPositions { get; set; } = string.Empty; // Правильные позиции знаков

        [StringLength(1000)]
        public string? PlainSentence { get; set; } // Обычное предложение
    }

    /// <summary>
    /// Вопрос для теста по орфоэпии
    /// </summary>
    public class OrthoeopyQuestion : Question
    {
        [Required]
        public int OrthoeopyTestId { get; set; }

        [Required]
        [StringLength(200)]
        public string Word { get; set; } = string.Empty; // Слово

        [Required]
        [Range(1, 20)]
        public int StressPosition { get; set; } // Позиция ударного слога (начиная с 1)

        [Required]
        [StringLength(200)]
        public string WordWithStress { get; set; } = string.Empty; // Слово с правильным ударением

        [StringLength(100)]
        public string? WrongStressPositions { get; set; } // Неправильные варианты (JSON массив позиций)
    }

    /// <summary>
    /// Вопрос для классического теста
    /// </summary>
    public class RegularQuestion : Question
    {
        [Required]
        public int RegularTestId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;

        [StringLength(1000)]
        public string? Explanation { get; set; } // Объяснение правильного ответа
    }

    /// <summary>
    /// Вопрос для теста на правописание частицы "не"
    /// </summary>
    public class NotParticleQuestion : Question
    {
        [Required]
        public int NotParticleTestId { get; set; }

        [Required]
        [StringLength(500)]
        public string TextWithGap { get; set; } = string.Empty; // Текст с (не) вместо частицы

        [Required]
        [StringLength(20)]
        public string CorrectAnswer { get; set; } = string.Empty; // "слитно" или "раздельно"

        [Required]
        [StringLength(500)]
        public string FullText { get; set; } = string.Empty; // Полный текст с правильным написанием
    }

    public enum QuestionType
    {
        SingleChoice = 1,      // Одиночный выбор
        MultipleChoice = 2,    // Множественный выбор
        TrueFalse = 3          // Верно/Неверно
    }
}

