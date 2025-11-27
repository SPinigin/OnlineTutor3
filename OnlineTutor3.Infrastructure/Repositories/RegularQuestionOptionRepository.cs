using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вариантами ответов классических тестов
    /// </summary>
    public class RegularQuestionOptionRepository : BaseRepository<RegularQuestionOption>, IRegularQuestionOptionRepository
    {
        public RegularQuestionOptionRepository(IDatabaseConnection db) : base(db, "RegularQuestionOptions")
        {
        }

        public async Task<List<RegularQuestionOption>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM RegularQuestionOptions WHERE RegularQuestionId = @QuestionId";
            return await _db.QueryAsync<RegularQuestionOption>(sql, new { QuestionId = questionId });
        }

        public async Task<List<RegularQuestionOption>> GetByQuestionIdOrderedAsync(int questionId)
        {
            var sql = "SELECT * FROM RegularQuestionOptions WHERE RegularQuestionId = @QuestionId ORDER BY OrderIndex";
            return await _db.QueryAsync<RegularQuestionOption>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT COUNT(*) FROM RegularQuestionOptions WHERE RegularQuestionId = @QuestionId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { QuestionId = questionId });
            return result ?? 0;
        }
    }
}

