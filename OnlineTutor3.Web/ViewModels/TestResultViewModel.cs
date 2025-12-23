using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// Базовый ViewModel для результатов тестов
    /// </summary>
    public class TestResultViewModel
    {
        public string TestTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public bool ShowCorrectAnswers { get; set; }
        public string TestIcon { get; set; } = string.Empty;
        public string TestColor { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
    }

    /// <summary>
    /// ViewModel для результата теста по орфографии
    /// </summary>
    public class SpellingTestResultViewModel : TestResultViewModel
    {
        public SpellingTestResult TestResult { get; set; } = null!;
        public SpellingTest Test { get; set; } = null!;
        public List<SpellingQuestion> Questions { get; set; } = new();
        public List<SpellingAnswer> Answers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для результата теста по пунктуации
    /// </summary>
    public class PunctuationTestResultViewModel : TestResultViewModel
    {
        public PunctuationTestResult TestResult { get; set; } = null!;
        public PunctuationTest Test { get; set; } = null!;
        public List<PunctuationQuestion> Questions { get; set; } = new();
        public List<PunctuationAnswer> Answers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для результата теста по орфоэпии
    /// </summary>
    public class OrthoeopyTestResultViewModel : TestResultViewModel
    {
        public OrthoeopyTestResult TestResult { get; set; } = null!;
        public OrthoeopyTest Test { get; set; } = null!;
        public List<OrthoeopyQuestion> Questions { get; set; } = new();
        public List<OrthoeopyAnswer> Answers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для результата классического теста
    /// </summary>
    public class RegularTestResultViewModel : TestResultViewModel
    {
        public RegularTestResult TestResult { get; set; } = null!;
        public RegularTest Test { get; set; } = null!;
        public List<RegularQuestion> Questions { get; set; } = new();
        public List<RegularQuestionOption> Options { get; set; } = new();
        public List<RegularAnswer> Answers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для результата теста на правописание частицы "не"
    /// </summary>
    public class NotParticleTestResultViewModel : TestResultViewModel
    {
        public NotParticleTestResult TestResult { get; set; } = null!;
        public NotParticleTest Test { get; set; } = null!;
        public List<NotParticleQuestion> Questions { get; set; } = new();
        public List<NotParticleAnswer> Answers { get; set; } = new();
    }
}

