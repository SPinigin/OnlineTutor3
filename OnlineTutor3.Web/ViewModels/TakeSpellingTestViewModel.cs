using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для прохождения теста по орфографии
    /// </summary>
    public class TakeSpellingTestViewModel
    {
        public SpellingTestResult TestResult { get; set; } = null!;
        public SpellingTest Test { get; set; } = null!;
        public List<SpellingQuestion> Questions { get; set; } = new();
        public List<SpellingAnswer> Answers { get; set; } = new();
        public TimeSpan TimeRemaining { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

    /// <summary>
    /// ViewModel для отправки ответа по орфографии
    /// </summary>
    public class SubmitSpellingAnswerViewModel
    {
        [Required]
        public int TestResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [StringLength(10)]
        public string? StudentAnswer { get; set; }

        /// <summary>
        /// Отмечен ли чекбокс "Буква не нужна"
        /// </summary>
        public bool NoLetterNeeded { get; set; } = false;
    }
}

