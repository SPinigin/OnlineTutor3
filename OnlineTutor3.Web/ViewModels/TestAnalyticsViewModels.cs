using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    // Base ViewModels
    public class CommonMistakeViewModel
    {
        public string IncorrectAnswer { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public List<string> StudentNames { get; set; } = new List<string>();
    }

    // Spelling Test Analytics
    public class SpellingTestAnalyticsViewModel
    {
        public SpellingTest SpellingTest { get; set; } = null!;
        public SpellingTestStatistics Statistics { get; set; } = null!;
        public List<SpellingStudentResultViewModel> SpellingResults { get; set; } = new List<SpellingStudentResultViewModel>();
        public List<SpellingQuestionAnalyticsViewModel> SpellingQuestionAnalytics { get; set; } = new List<SpellingQuestionAnalyticsViewModel>();
        public List<Student> StudentsNotTaken { get; set; } = new List<Student>();
    }

    public class SpellingTestStatistics
    {
        public int TotalStudents { get; set; }
        public int StudentsCompleted { get; set; }
        public int StudentsNotStarted { get; set; }
        public int StudentsInProgress { get; set; }
        public double AverageScore { get; set; }
        public double AveragePercentage { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
        public DateTime? FirstCompletion { get; set; }
        public DateTime? LastCompletion { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class SpellingStudentResultViewModel
    {
        public Student Student { get; set; } = null!;
        public List<SpellingTestResult> Results { get; set; } = new List<SpellingTestResult>();
        public SpellingTestResult? BestResult { get; set; }
        public SpellingTestResult? LatestResult { get; set; }
        public int AttemptsUsed { get; set; }
        public bool HasCompleted { get; set; }
        public bool IsInProgress { get; set; }
        public TimeSpan? TotalTimeSpent { get; set; }
    }

    public class SpellingQuestionAnalyticsViewModel
    {
        public SpellingQuestion SpellingQuestion { get; set; } = null!;
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double SuccessRate { get; set; }
        public List<CommonMistakeViewModel> CommonMistakes { get; set; } = new List<CommonMistakeViewModel>();
        public bool IsMostDifficult { get; set; }
        public bool IsEasiest { get; set; }
    }

    // Punctuation Test Analytics
    public class PunctuationTestAnalyticsViewModel
    {
        public PunctuationTest PunctuationTest { get; set; } = null!;
        public PunctuationTestStatistics Statistics { get; set; } = null!;
        public List<PunctuationStudentResultViewModel> StudentResults { get; set; } = new List<PunctuationStudentResultViewModel>();
        public List<PunctuationQuestionAnalyticsViewModel> QuestionAnalytics { get; set; } = new List<PunctuationQuestionAnalyticsViewModel>();
        public List<Student> StudentsNotTaken { get; set; } = new List<Student>();
    }

    public class PunctuationTestStatistics
    {
        public int TotalStudents { get; set; }
        public int StudentsCompleted { get; set; }
        public int StudentsNotStarted { get; set; }
        public int StudentsInProgress { get; set; }
        public double AverageScore { get; set; }
        public double AveragePercentage { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
        public DateTime? FirstCompletion { get; set; }
        public DateTime? LastCompletion { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class PunctuationStudentResultViewModel
    {
        public Student Student { get; set; } = null!;
        public List<PunctuationTestResult> Results { get; set; } = new List<PunctuationTestResult>();
        public PunctuationTestResult? BestResult { get; set; }
        public PunctuationTestResult? LatestResult { get; set; }
        public int AttemptsUsed { get; set; }
        public bool HasCompleted { get; set; }
        public bool IsInProgress { get; set; }
        public TimeSpan? TotalTimeSpent { get; set; }
    }

    public class PunctuationQuestionAnalyticsViewModel
    {
        public PunctuationQuestion PunctuationQuestion { get; set; } = null!;
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double SuccessRate { get; set; }
        public List<CommonMistakeViewModel> CommonMistakes { get; set; } = new List<CommonMistakeViewModel>();
        public bool IsMostDifficult { get; set; }
        public bool IsEasiest { get; set; }
    }

    // Orthoeopy Test Analytics
    public class OrthoeopyTestAnalyticsViewModel
    {
        public OrthoeopyTest OrthoeopyTest { get; set; } = null!;
        public OrthoeopyTestStatistics Statistics { get; set; } = null!;
        public List<OrthoeopyStudentResultViewModel> StudentResults { get; set; } = new List<OrthoeopyStudentResultViewModel>();
        public List<OrthoeopyQuestionAnalyticsViewModel> QuestionAnalytics { get; set; } = new List<OrthoeopyQuestionAnalyticsViewModel>();
        public List<Student> StudentsNotTaken { get; set; } = new List<Student>();
    }

    public class OrthoeopyTestStatistics
    {
        public int TotalStudents { get; set; }
        public int StudentsCompleted { get; set; }
        public int StudentsNotStarted { get; set; }
        public int StudentsInProgress { get; set; }
        public double AverageScore { get; set; }
        public double AveragePercentage { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
        public DateTime? FirstCompletion { get; set; }
        public DateTime? LastCompletion { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class OrthoeopyStudentResultViewModel
    {
        public Student Student { get; set; } = null!;
        public List<OrthoeopyTestResult> Results { get; set; } = new List<OrthoeopyTestResult>();
        public OrthoeopyTestResult? BestResult { get; set; }
        public OrthoeopyTestResult? LatestResult { get; set; }
        public int AttemptsUsed { get; set; }
        public bool HasCompleted { get; set; }
        public bool IsInProgress { get; set; }
        public TimeSpan? TotalTimeSpent { get; set; }
    }

    public class OrthoeopyQuestionAnalyticsViewModel
    {
        public OrthoeopyQuestion OrthoeopyQuestion { get; set; } = null!;
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double SuccessRate { get; set; }
        public List<StressPositionMistakeViewModel> CommonMistakes { get; set; } = new List<StressPositionMistakeViewModel>();
        public bool IsMostDifficult { get; set; }
        public bool IsEasiest { get; set; }
    }

    public class StressPositionMistakeViewModel
    {
        public int IncorrectPosition { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public List<string> StudentNames { get; set; } = new List<string>();
    }

    // Regular Test Analytics
    public class RegularTestAnalyticsViewModel
    {
        public RegularTest RegularTest { get; set; } = null!;
        public RegularTestStatistics Statistics { get; set; } = null!;
        public List<RegularTestStudentResultViewModel> RegularResults { get; set; } = new List<RegularTestStudentResultViewModel>();
        public List<RegularTestQuestionAnalyticsViewModel> QuestionAnalytics { get; set; } = new List<RegularTestQuestionAnalyticsViewModel>();
        public List<Student> StudentsNotTaken { get; set; } = new List<Student>();
    }

    public class RegularTestStatistics
    {
        public int TotalStudents { get; set; }
        public int StudentsCompleted { get; set; }
        public int StudentsNotStarted { get; set; }
        public int StudentsInProgress { get; set; }
        public double AverageScore { get; set; }
        public double AveragePercentage { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
        public DateTime? FirstCompletion { get; set; }
        public DateTime? LastCompletion { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class RegularTestStudentResultViewModel
    {
        public Student Student { get; set; } = null!;
        public List<RegularTestResult> Results { get; set; } = new List<RegularTestResult>();
        public RegularTestResult? BestResult { get; set; }
        public RegularTestResult? LatestResult { get; set; }
        public int AttemptsUsed { get; set; }
        public bool HasCompleted { get; set; }
        public bool IsInProgress { get; set; }
        public TimeSpan? TotalTimeSpent { get; set; }
    }

    public class RegularTestQuestionAnalyticsViewModel
    {
        public RegularQuestion RegularQuestion { get; set; } = null!;
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double SuccessRate { get; set; }
        public List<CommonMistakeViewModel> CommonMistakes { get; set; } = new List<CommonMistakeViewModel>();
        public bool IsMostDifficult { get; set; }
        public bool IsEasiest { get; set; }
    }
}

