using OnlineTutor3.Application.DTOs;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для списка доступных тестов студента
    /// </summary>
    public class StudentTestIndexViewModel
    {
        public Student Student { get; set; } = null!;
        public Class? Class { get; set; }
        public string? CurrentCategory { get; set; }
        public string? SearchQuery { get; set; }
        public int? AssignmentFilter { get; set; }

        public List<AvailableTestInfo> SpellingTests { get; set; } = new();
        public List<AvailableTestInfo> PunctuationTests { get; set; } = new();
        public List<AvailableTestInfo> OrthoeopyTests { get; set; } = new();
        public List<AvailableTestInfo> RegularTests { get; set; } = new();
        public List<AvailableTestInfo> NotParticleTests { get; set; } = new();

        public List<Assignment> AvailableAssignments { get; set; } = new();
        
        // Группировка тестов по заданиям
        public Dictionary<int, AssignmentTestsInfo> AssignmentsWithTests { get; set; } = new();
        public Dictionary<int, string> SubjectsDict { get; set; } = new();
    }

    /// <summary>
    /// Информация о тестах в задании
    /// </summary>
    public class AssignmentTestsInfo
    {
        public Assignment Assignment { get; set; } = null!;
        public List<AvailableTestInfo> SpellingTests { get; set; } = new();
        public List<AvailableTestInfo> PunctuationTests { get; set; } = new();
        public List<AvailableTestInfo> OrthoeopyTests { get; set; } = new();
        public List<AvailableTestInfo> RegularTests { get; set; } = new();
        public List<AvailableTestInfo> NotParticleTests { get; set; } = new();

        public int TotalTestsCount => SpellingTests.Count + PunctuationTests.Count + OrthoeopyTests.Count + RegularTests.Count + NotParticleTests.Count;
    }

    /// <summary>
    /// Информация о доступном тесте с статусом
    /// </summary>
    public class AvailableTestInfo
    {
        // Базовая информация о тесте
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TestType { get; set; } = string.Empty; // "Spelling", "Punctuation", "Orthoeopy", "Regular", "NotParticle"
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int QuestionsCount { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;

        // Статус и результаты
        public TestStatus Status { get; set; }
        public int AttemptsUsed { get; set; }
        public int? OngoingTestResultId { get; set; }
        public double? BestPercentage { get; set; }
        public int? BestScore { get; set; }
        public int? BestMaxScore { get; set; }
    }

    /// <summary>
    /// Статус теста для студента
    /// </summary>
    public enum TestStatus
    {
        CanStart,      // Можно начать
        Ongoing,       // В процессе (есть незавершенная попытка)
        Exhausted,     // Исчерпаны попытки
        NotAvailable   // Недоступен (не назначен классу или вне дат)
    }
}

