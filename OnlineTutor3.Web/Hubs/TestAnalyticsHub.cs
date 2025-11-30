using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.Hubs
{
    /// <summary>
    /// SignalR Hub для отслеживания активности студентов в реальном времени
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class TestAnalyticsHub : Hub
    {
        private readonly ILogger<TestAnalyticsHub> _logger;

        public TestAnalyticsHub(ILogger<TestAnalyticsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            _logger.LogInformation("SignalR: Пользователь {UserId} подключился. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Присоединение к группе конкретного теста (для страницы аналитики теста)
        /// </summary>
        public async Task JoinTestAnalytics(int testId, string testType)
        {
            var groupName = $"{testType}_test_{testId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("SignalR: ConnectionId {ConnectionId} присоединился к группе теста {GroupName}",
                Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Покидание группы конкретного теста
        /// </summary>
        public async Task LeaveTestAnalytics(int testId, string testType)
        {
            var groupName = $"{testType}_test_{testId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("SignalR: ConnectionId {ConnectionId} покинул группу теста {GroupName}",
                Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Присоединение к глобальной группе учителя (для Dashboard)
        /// </summary>
        public async Task JoinTeacherDashboard(string teacherId)
        {
            if (string.IsNullOrEmpty(teacherId))
            {
                _logger.LogWarning("SignalR: Попытка присоединения к Dashboard с пустым teacherId");
                return;
            }

            var groupName = $"teacher_{teacherId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("SignalR: ConnectionId {ConnectionId} присоединился к Dashboard учителя {TeacherId}",
                Context.ConnectionId, teacherId);
        }

        /// <summary>
        /// Покидание глобальной группы учителя
        /// </summary>
        public async Task LeaveTeacherDashboard(string teacherId)
        {
            if (string.IsNullOrEmpty(teacherId))
            {
                return;
            }

            var groupName = $"teacher_{teacherId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("SignalR: ConnectionId {ConnectionId} покинул Dashboard учителя {TeacherId}",
                Context.ConnectionId, teacherId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            _logger.LogInformation("SignalR: Пользователь {UserId} отключился. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);

            if (exception != null)
            {
                _logger.LogError(exception, "SignalR: Ошибка при отключении");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

