using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Infrastructure.Data;

namespace OnlineTutor3.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с материалами
    /// </summary>
    public class MaterialRepository : BaseRepository<Material>, IMaterialRepository
    {
        public MaterialRepository(IDatabaseConnection db) : base(db, "Materials")
        {
        }

        public async Task<List<Material>> GetByClassIdAsync(int classId)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE ClassId = @ClassId 
                ORDER BY UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { ClassId = classId });
        }

        public async Task<List<Material>> GetByAssignmentIdAsync(int assignmentId)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE AssignmentId = @AssignmentId 
                ORDER BY UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<Material>> GetByUploadedByIdAsync(string uploadedById)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE UploadedById = @UploadedById 
                ORDER BY UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { UploadedById = uploadedById });
        }

        public async Task<List<Material>> GetActiveByClassIdAsync(int classId)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE ClassId = @ClassId AND IsActive = 1 
                ORDER BY UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { ClassId = classId });
        }

        public async Task<List<Material>> GetActiveByAssignmentIdAsync(int assignmentId)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE AssignmentId = @AssignmentId AND IsActive = 1 
                ORDER BY UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { AssignmentId = assignmentId });
        }

        public async Task<List<Material>> GetFilteredAsync(
            string? uploadedById,
            string? searchString = null,
            int? classFilter = null,
            int? assignmentFilter = null,
            MaterialType? typeFilter = null,
            string? sortOrder = null)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE (@UploadedById IS NULL OR UploadedById = @UploadedById)
                    AND (@SearchString IS NULL OR Title LIKE '%' + @SearchString + '%' OR Description LIKE '%' + @SearchString + '%')
                    AND (@ClassFilter IS NULL OR ClassId = @ClassFilter)
                    AND (@AssignmentFilter IS NULL OR AssignmentId = @AssignmentFilter)
                    AND (@TypeFilter IS NULL OR Type = @TypeFilter)";

            // Добавляем сортировку
            switch (sortOrder)
            {
                case "title_desc":
                    sql += " ORDER BY Title DESC";
                    break;
                case "Date":
                    sql += " ORDER BY UploadedAt ASC";
                    break;
                case "date_desc":
                    sql += " ORDER BY UploadedAt DESC";
                    break;
                case "Size":
                    sql += " ORDER BY FileSize ASC";
                    break;
                case "size_desc":
                    sql += " ORDER BY FileSize DESC";
                    break;
                default:
                    sql += " ORDER BY UploadedAt DESC";
                    break;
            }

            return await _db.QueryAsync<Material>(sql, new
            {
                UploadedById = uploadedById,
                SearchString = searchString,
                ClassFilter = classFilter,
                AssignmentFilter = assignmentFilter,
                TypeFilter = typeFilter.HasValue ? (int?)typeFilter.Value : null
            });
        }

        public async Task<Material?> GetByIdWithDetailsAsync(int id, string uploadedById)
        {
            var sql = @"
                SELECT * FROM Materials 
                WHERE Id = @Id AND UploadedById = @UploadedById";
            
            return await _db.QueryFirstOrDefaultAsync<Material>(sql, new { Id = id, UploadedById = uploadedById });
        }

        public async Task<List<Material>> GetAvailableForStudentAsync(int studentId)
        {
            // Получаем материалы для классов студента и/или заданий, назначенных классам студента
            var sql = @"
                SELECT DISTINCT m.*
                FROM Materials m
                INNER JOIN Students s ON s.Id = @StudentId
                LEFT JOIN Classes c ON m.ClassId = c.Id
                LEFT JOIN AssignmentClasses ac ON m.AssignmentId = ac.AssignmentId
                WHERE m.IsActive = 1
                    AND (
                        (m.ClassId IS NOT NULL AND m.ClassId = s.ClassId)
                        OR (m.AssignmentId IS NOT NULL AND ac.ClassId = s.ClassId)
                    )
                ORDER BY m.UploadedAt DESC";
            
            return await _db.QueryAsync<Material>(sql, new { StudentId = studentId });
        }

        public override async Task<int> CreateAsync(Material entity)
        {
            var sql = @"
                INSERT INTO Materials (
                    Title, Description, FilePath, FileName, FileSize, ContentType, Type,
                    ClassId, AssignmentId, UploadedById, UploadedAt, IsActive
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @Title, @Description, @FilePath, @FileName, @FileSize, @ContentType, @Type,
                    @ClassId, @AssignmentId, @UploadedById, @UploadedAt, @IsActive
                )";
            
            var id = await _db.QueryScalarAsync<int>(sql, new
            {
                entity.Title,
                entity.Description,
                entity.FilePath,
                entity.FileName,
                entity.FileSize,
                entity.ContentType,
                Type = (int)entity.Type,
                entity.ClassId,
                entity.AssignmentId,
                entity.UploadedById,
                entity.UploadedAt,
                entity.IsActive
            });
            
            return id;
        }

        public override async Task<int> UpdateAsync(Material entity)
        {
            var sql = @"
                UPDATE Materials 
                SET Title = @Title, 
                    Description = @Description, 
                    ClassId = @ClassId,
                    AssignmentId = @AssignmentId,
                    IsActive = @IsActive
                WHERE Id = @Id";
            
            return await _db.ExecuteAsync(sql, new
            {
                entity.Id,
                entity.Title,
                entity.Description,
                entity.ClassId,
                entity.AssignmentId,
                entity.IsActive
            });
        }
    }
}

