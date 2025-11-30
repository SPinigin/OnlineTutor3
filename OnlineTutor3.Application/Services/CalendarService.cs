using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с календарем
    /// </summary>
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly ILogger<CalendarService> _logger;

        public CalendarService(
            ICalendarEventRepository calendarEventRepository,
            ILogger<CalendarService> logger)
        {
            _calendarEventRepository = calendarEventRepository;
            _logger = logger;
        }

        public async Task<CalendarEvent?> GetByIdAsync(int id)
        {
            try
            {
                return await _calendarEventRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении события календаря по ID: {EventId}", id);
                throw;
            }
        }

        public async Task<List<CalendarEvent>> GetByTeacherIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(teacherId));
                }

                return await _calendarEventRepository.GetByTeacherIdAsync(teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий календаря учителя: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<List<CalendarEvent>> GetByClassIdAsync(int classId)
        {
            try
            {
                return await _calendarEventRepository.GetByClassIdAsync(classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий календаря класса: {ClassId}", classId);
                throw;
            }
        }

        public async Task<List<CalendarEvent>> GetByStudentIdAsync(int studentId)
        {
            try
            {
                return await _calendarEventRepository.GetByStudentIdAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий календаря ученика: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<List<CalendarEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _calendarEventRepository.GetByDateRangeAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий календаря в диапазоне дат: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<int> GetUpcomingCountAsync(string teacherId, DateTime now)
        {
            try
            {
                return await _calendarEventRepository.GetUpcomingCountAsync(teacherId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества предстоящих событий: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> GetTodayCountAsync(string teacherId, DateTime now)
        {
            try
            {
                return await _calendarEventRepository.GetTodayCountAsync(teacherId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества событий сегодня: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<int> GetCompletedThisMonthCountAsync(string teacherId, DateTime now)
        {
            try
            {
                return await _calendarEventRepository.GetCompletedThisMonthCountAsync(teacherId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении количества завершенных событий за месяц: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<List<CalendarEvent>> GetByTeacherIdInDateRangeAsync(string teacherId, DateTime? start, DateTime? end)
        {
            try
            {
                return await _calendarEventRepository.GetByTeacherIdInDateRangeAsync(teacherId, start, end);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении событий календаря учителя в диапазоне: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<CalendarEvent?> GetByIdWithRelationsAsync(int id, string teacherId)
        {
            try
            {
                return await _calendarEventRepository.GetByIdWithRelationsAsync(id, teacherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении события календаря с связями: {EventId}, {TeacherId}", id, teacherId);
                throw;
            }
        }

        public async Task<int> CreateAsync(CalendarEvent calendarEvent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(calendarEvent.TeacherId))
                {
                    throw new ArgumentException("TeacherId не может быть пустым", nameof(calendarEvent));
                }

                if (calendarEvent.EndDateTime <= calendarEvent.StartDateTime)
                {
                    throw new ArgumentException("Время окончания должно быть позже времени начала", nameof(calendarEvent));
                }

                calendarEvent.CreatedAt = DateTime.Now;
                return await _calendarEventRepository.CreateAsync(calendarEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании события календаря: {TeacherId}", calendarEvent.TeacherId);
                throw;
            }
        }

        public async Task<int> UpdateAsync(CalendarEvent calendarEvent)
        {
            try
            {
                if (calendarEvent.EndDateTime <= calendarEvent.StartDateTime)
                {
                    throw new ArgumentException("Время окончания должно быть позже времени начала", nameof(calendarEvent));
                }

                calendarEvent.UpdatedAt = DateTime.Now;
                return await _calendarEventRepository.UpdateAsync(calendarEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении события календаря: {EventId}", calendarEvent.Id);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                return await _calendarEventRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении события календаря: {EventId}", id);
                throw;
            }
        }

        public async Task<bool> TeacherCanAccessEventAsync(string teacherId, int eventId)
        {
            try
            {
                var calendarEvent = await _calendarEventRepository.GetByIdWithRelationsAsync(eventId, teacherId);
                return calendarEvent != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа учителя к событию: {TeacherId}, {EventId}", teacherId, eventId);
                return false;
            }
        }
    }
}

