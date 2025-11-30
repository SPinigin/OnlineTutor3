namespace OnlineTutor3.Application.Interfaces
{
    /// <summary>
    /// Сервис для кэширования данных
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получить значение из кэша
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Сохранить значение в кэш
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Удалить значение из кэша
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Очистить весь кэш
        /// </summary>
        Task ClearAsync();
    }
}

