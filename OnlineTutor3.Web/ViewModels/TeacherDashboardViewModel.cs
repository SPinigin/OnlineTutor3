using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для панели мониторинга учителя
    /// </summary>
    public class TeacherDashboardViewModel
    {
        public ApplicationUser Teacher { get; set; } = null!;
        
        public List<SpellingTest> SpellingTests { get; set; } = new();
        public List<PunctuationTest> PunctuationTests { get; set; } = new();
        public List<OrthoeopyTest> OrthoeopyTests { get; set; } = new();
        public List<RegularTest> RegularTests { get; set; } = new();
        public List<NotParticleTest> NotParticleTests { get; set; } = new();

        public int TotalActiveTests => SpellingTests.Count + PunctuationTests.Count + 
                                      OrthoeopyTests.Count + RegularTests.Count + NotParticleTests.Count;

        public int TotalStudentsInProgress { get; set; }
        public int TotalCompletedToday { get; set; }

        /// <summary>
        /// Статистика по тестам: ключ - "testType_testId", значение - (Completed, InProgress)
        /// </summary>
        public Dictionary<string, (int Completed, int InProgress)> TestStatistics { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для активности теста
    /// </summary>
    public class TestActivityViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty; // spelling, punctuation, orthoeopy, regular, notparticle
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // started, continued, completed, in_progress
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public int? TestResultId { get; set; }
        public bool IsAutoCompleted { get; set; }
    }
}

