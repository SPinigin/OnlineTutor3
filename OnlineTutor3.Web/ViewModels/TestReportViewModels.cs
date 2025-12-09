using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для страницы со списком отчетов
    /// </summary>
    public class TestReportIndexViewModel
    {
        public List<TestReportItemViewModel> TestReports { get; set; } = new List<TestReportItemViewModel>();
        public Dictionary<int, string> SubjectsDict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> AssignmentsDict { get; set; } = new Dictionary<int, string>();
        public int? SelectedSubjectId { get; set; }
        public int? SelectedClassId { get; set; }
    }

    /// <summary>
    /// ViewModel для одного теста в списке отчетов
    /// </summary>
    public class TestReportItemViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty; // "spelling", "punctuation", "orthoeopy", "regular"
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int NotStartedCount { get; set; }
        public double AveragePercentage { get; set; }
        public int? AverageGrade { get; set; }
        public DateTime? LastCompletionDate { get; set; }
    }

    /// <summary>
    /// ViewModel для детального отчета по тесту
    /// </summary>
    public class TestReportDetailViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string TestDescription { get; set; } = string.Empty;
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public TestReportStatistics Statistics { get; set; } = new TestReportStatistics();
        public List<TestReportStudentViewModel> StudentReports { get; set; } = new List<TestReportStudentViewModel>();
    }

    /// <summary>
    /// Статистика по тесту
    /// </summary>
    public class TestReportStatistics
    {
        public int TotalStudents { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int NotStartedCount { get; set; }
        public double AverageScore { get; set; }
        public double AveragePercentage { get; set; }
        public int? AverageGrade { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public int MaxScore { get; set; }
        public DateTime? FirstCompletionDate { get; set; }
        public DateTime? LastCompletionDate { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// ViewModel для отчета по ученику в тесте
    /// </summary>
    public class TestReportStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public int AttemptsCount { get; set; }
        public bool HasCompleted { get; set; }
        public bool IsInProgress { get; set; }
        public int? BestScore { get; set; }
        public int? BestPercentage { get; set; }
        public int? BestGrade { get; set; }
        public int? LatestScore { get; set; }
        public int? LatestPercentage { get; set; }
        public int? LatestGrade { get; set; }
        public DateTime? FirstAttemptDate { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public DateTime? LastCompletionDate { get; set; }
    }
}

