using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для страницы материалов студента
    /// </summary>
    public class StudentMaterialIndexViewModel
    {
        public Student Student { get; set; } = null!;
        public Dictionary<int, List<Material>> MaterialsByAssignment { get; set; } = new Dictionary<int, List<Material>>();
        public Dictionary<int, Assignment> AssignmentsDict { get; set; } = new Dictionary<int, Assignment>();
        public List<Material> MaterialsWithoutAssignment { get; set; } = new List<Material>();
        public Dictionary<int, string> SubjectsDict { get; set; } = new Dictionary<int, string>();
        public string? SearchQuery { get; set; }
    }
}

