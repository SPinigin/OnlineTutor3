using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.DTOs
{
    /// <summary>
    /// Данные для главной страницы студента
    /// </summary>
    public class StudentDashboardData
    {
        public Student Student { get; set; } = null!;
        public Class? Class { get; set; }

        // Статистика тестов
        public int TotalTestsCompleted { get; set; }
        public int TotalTestsAvailable { get; set; }
        public double AveragePercentage { get; set; }
        public int TotalPoints { get; set; }

        // Последние результаты
        public List<TestResultSummary> RecentResults { get; set; } = new();

        // Ближайшие дедлайны
        public List<AssignmentDeadline> UpcomingDeadlines { get; set; } = new();
    }

    /// <summary>
    /// Краткая информация о результате теста
    /// </summary>
    public class TestResultSummary
    {
        public int Id { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int? Grade { get; set; }
        public DateTime CompletedAt { get; set; }
        public int AttemptNumber { get; set; }
    }

    /// <summary>
    /// Информация о дедлайне задания
    /// </summary>
    public class AssignmentDeadline
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int TestsCount { get; set; }
        public int CompletedTestsCount { get; set; }
    }
}

