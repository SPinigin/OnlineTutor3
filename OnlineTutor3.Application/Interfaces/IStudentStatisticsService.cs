using OnlineTutor3.Application.DTOs;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для получения статистики студента
    /// </summary>
    public interface IStudentStatisticsService
    {
        /// <summary>
        /// Получает статистику для главной страницы студента
        /// </summary>
        Task<StudentDashboardData> GetDashboardDataAsync(int studentId);

        /// <summary>
        /// Получает количество завершенных тестов
        /// </summary>
        Task<int> GetCompletedTestsCountAsync(int studentId);

        /// <summary>
        /// Получает количество доступных тестов
        /// </summary>
        Task<int> GetAvailableTestsCountAsync(int studentId);

        /// <summary>
        /// Получает средний процент выполнения тестов
        /// </summary>
        Task<double> GetAveragePercentageAsync(int studentId);

        /// <summary>
        /// Получает общее количество набранных баллов
        /// </summary>
        Task<int> GetTotalPointsAsync(int studentId);
    }
}

