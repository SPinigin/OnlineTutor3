using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для главной страницы учителя
    /// </summary>
    public class TeacherIndexViewModel
    {
        public ApplicationUser Teacher { get; set; } = null!;
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalActiveAssignments { get; set; }
        public int TotalActiveTests { get; set; }
        public List<RecentTestCompletionViewModel> RecentCompletions { get; set; } = new List<RecentTestCompletionViewModel>();
        public Dictionary<int, string> SubjectsDict { get; set; } = new Dictionary<int, string>();
    }
}

