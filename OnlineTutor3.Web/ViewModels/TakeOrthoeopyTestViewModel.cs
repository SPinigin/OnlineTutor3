using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для прохождения теста по орфоэпии
    /// </summary>
    public class TakeOrthoeopyTestViewModel
    {
        public OrthoeopyTestResult TestResult { get; set; } = null!;
        public OrthoeopyTest Test { get; set; } = null!;
        public List<OrthoeopyQuestion> Questions { get; set; } = new();
        public List<OrthoeopyAnswer> Answers { get; set; } = new();
        public TimeSpan TimeRemaining { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

    /// <summary>
    /// ViewModel для отправки ответа по орфоэпии
    /// </summary>
    public class SubmitOrthoeopyAnswerViewModel
    {
        [Required]
        public int TestResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int SelectedStressPosition { get; set; }
    }
}

