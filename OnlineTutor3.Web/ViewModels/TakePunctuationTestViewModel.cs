using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для прохождения теста по пунктуации
    /// </summary>
    public class TakePunctuationTestViewModel
    {
        public PunctuationTestResult TestResult { get; set; } = null!;
        public PunctuationTest Test { get; set; } = null!;
        public List<PunctuationQuestion> Questions { get; set; } = new();
        public List<PunctuationAnswer> Answers { get; set; } = new();
        public TimeSpan TimeRemaining { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

    /// <summary>
    /// ViewModel для отправки ответа по пунктуации
    /// </summary>
    public class SubmitPunctuationAnswerViewModel
    {
        [Required]
        public int TestResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [StringLength(50)]
        public string? StudentAnswer { get; set; }
    }
}

