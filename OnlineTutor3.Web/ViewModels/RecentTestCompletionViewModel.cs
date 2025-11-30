namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для отображения последних прохождений тестов
    /// </summary>
    public class RecentTestCompletionViewModel
    {
        public int TestResultId { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int? Grade { get; set; }
        public DateTime CompletedAt { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
}

