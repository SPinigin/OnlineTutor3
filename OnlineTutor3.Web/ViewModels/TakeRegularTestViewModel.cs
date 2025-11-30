using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для прохождения классического теста
    /// </summary>
    public class TakeRegularTestViewModel
    {
        public RegularTestResult TestResult { get; set; } = null!;
        public RegularTest Test { get; set; } = null!;
        public List<RegularQuestion> Questions { get; set; } = new();
        public List<RegularQuestionOption> Options { get; set; } = new();
        public List<RegularAnswer> Answers { get; set; } = new();
        public TimeSpan TimeRemaining { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

    /// <summary>
    /// ViewModel для отправки ответа на классический тест
    /// </summary>
    public class SubmitRegularAnswerViewModel
    {
        [Required]
        public int TestResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string? StudentAnswer { get; set; }

        public int? SelectedOptionId { get; set; }

        public List<int>? SelectedOptionIds { get; set; } // Для MultipleChoice
    }
}

