using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Базовый репозиторий с общими методами
    /// </summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly IDatabaseConnection _db;
        protected readonly string _tableName;
        protected readonly string _idColumn;

        protected BaseRepository(IDatabaseConnection db, string tableName, string idColumn = "Id")
        {
            _db = db;
            _tableName = tableName;
            _idColumn = idColumn;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            var sql = $"SELECT * FROM {_tableName} WHERE {_idColumn} = @Id";
            return await _db.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var sql = $"SELECT * FROM {_tableName}";
            return await _db.QueryAsync<T>(sql);
        }

        public virtual async Task<int> CreateAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _idColumn && p.CanWrite && IsDatabaseColumn(p))
                .ToList();

            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) OUTPUT INSERTED.{_idColumn} VALUES ({values})";
            
            var id = await _db.QueryScalarAsync<int>(sql, entity);
            return id;
        }

        /// <summary>
        /// Проверяет, является ли свойство столбцом базы данных (исключает навигационные свойства)
        /// </summary>
        private static bool IsDatabaseColumn(System.Reflection.PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            
            // Получаем реальный тип (если это Nullable, берем базовый тип)
            var actualType = propertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            }
            
            // Исключаем коллекции (навигационные свойства для one-to-many)
            if (actualType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(actualType))
            {
                return false;
            }
            
            // Разрешаем только примитивные типы, string, DateTime, decimal, Guid и их Nullable версии
            if (actualType.IsPrimitive || 
                actualType == typeof(string) || 
                actualType == typeof(DateTime) || 
                actualType == typeof(decimal) || 
                actualType == typeof(Guid) ||
                actualType == typeof(byte[]) ||
                actualType.IsEnum)
            {
                return true;
            }
            
            // Все остальные классы исключаем (это навигационные свойства)
            return false;
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _idColumn && p.CanWrite && IsDatabaseColumn(p))
                .ToList();

            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
            var sql = $"UPDATE {_tableName} SET {setClause} WHERE {_idColumn} = @{_idColumn}";

            return await _db.ExecuteAsync(sql, entity);
        }

        public virtual async Task<int> DeleteAsync(int id)
        {
            var sql = $"DELETE FROM {_tableName} WHERE {_idColumn} = @Id";
            return await _db.ExecuteAsync(sql, new { Id = id });
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE {_idColumn} = @Id";
            var count = await _db.QueryScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}

