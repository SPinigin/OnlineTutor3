using System.ComponentModel.DataAnnotations;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для прохождения теста на правописание частицы "не"
    /// </summary>
    public class TakeNotParticleTestViewModel
    {
        public NotParticleTestResult TestResult { get; set; } = null!;
        public NotParticleTest Test { get; set; } = null!;
        public List<NotParticleQuestion> Questions { get; set; } = new();
        public List<NotParticleAnswer> Answers { get; set; } = new();
        public TimeSpan TimeRemaining { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

    /// <summary>
    /// ViewModel для отправки ответа на тест на правописание частицы "не"
    /// </summary>
    public class SubmitNotParticleAnswerViewModel
    {
        [Required]
        public int TestResultId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentAnswer { get; set; } = string.Empty; // "слитно" или "раздельно"
    }
}

