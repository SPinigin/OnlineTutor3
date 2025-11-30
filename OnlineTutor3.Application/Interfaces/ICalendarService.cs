using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с календарем
    /// </summary>
    public interface ICalendarService
    {
        Task<CalendarEvent?> GetByIdAsync(int id);
        Task<List<CalendarEvent>> GetByTeacherIdAsync(string teacherId);
        Task<List<CalendarEvent>> GetByClassIdAsync(int classId);
        Task<List<CalendarEvent>> GetByStudentIdAsync(int studentId);
        Task<List<CalendarEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetUpcomingCountAsync(string teacherId, DateTime now);
        Task<int> GetTodayCountAsync(string teacherId, DateTime now);
        Task<int> GetCompletedThisMonthCountAsync(string teacherId, DateTime now);
        Task<List<CalendarEvent>> GetByTeacherIdInDateRangeAsync(string teacherId, DateTime? start, DateTime? end);
        Task<CalendarEvent?> GetByIdWithRelationsAsync(int id, string teacherId);
        Task<int> CreateAsync(CalendarEvent calendarEvent);
        Task<int> UpdateAsync(CalendarEvent calendarEvent);
        Task<int> DeleteAsync(int id);
        Task<bool> TeacherCanAccessEventAsync(string teacherId, int eventId);
    }
}

