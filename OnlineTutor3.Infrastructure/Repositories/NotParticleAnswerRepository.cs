using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с ответами на вопросы теста на правописание частицы "не"
    /// </summary>
    public class NotParticleAnswerRepository : BaseRepository<NotParticleAnswer>, INotParticleAnswerRepository
    {
        public NotParticleAnswerRepository(IDatabaseConnection db) : base(db, "NotParticleAnswers")
        {
        }

        public async Task<List<NotParticleAnswer>> GetByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT * FROM NotParticleAnswers WHERE TestResultId = @TestResultId ORDER BY Id";
            return await _db.QueryAsync<NotParticleAnswer>(sql, new { TestResultId = testResultId });
        }

        public async Task<List<NotParticleAnswer>> GetByQuestionIdAsync(int questionId)
        {
            var sql = "SELECT * FROM NotParticleAnswers WHERE NotParticleQuestionId = @QuestionId ORDER BY Id";
            return await _db.QueryAsync<NotParticleAnswer>(sql, new { QuestionId = questionId });
        }

        public async Task<int> GetCountByTestResultIdAsync(int testResultId)
        {
            var sql = "SELECT COUNT(*) FROM NotParticleAnswers WHERE TestResultId = @TestResultId";
            var result = await _db.QueryScalarAsync<int?>(sql, new { TestResultId = testResultId });
            return result ?? 0;
        }
    }
}

