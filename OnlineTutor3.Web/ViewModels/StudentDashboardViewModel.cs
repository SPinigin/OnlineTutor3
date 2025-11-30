using OnlineTutor3.Application.DTOs;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для главной страницы студента
    /// </summary>
    public class StudentDashboardViewModel
    {
        public StudentDashboardData Data { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}

