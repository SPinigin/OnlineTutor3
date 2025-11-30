using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для работы с материалами
    /// </summary>
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IClassService _classService;
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(
            IMaterialRepository materialRepository,
            IClassService classService,
            IAssignmentService assignmentService,
            ILogger<MaterialService> logger)
        {
            _materialRepository = materialRepository;
            _classService = classService;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task<Material?> GetByIdAsync(int id)
        {
            return await _materialRepository.GetByIdAsync(id);
        }

        public async Task<List<Material>> GetByTeacherIdAsync(string teacherId)
        {
            return await _materialRepository.GetByUploadedByIdAsync(teacherId);
        }

        public async Task<List<Material>> GetByClassIdAsync(int classId)
        {
            return await _materialRepository.GetByClassIdAsync(classId);
        }

        public async Task<List<Material>> GetByAssignmentIdAsync(int assignmentId)
        {
            return await _materialRepository.GetByAssignmentIdAsync(assignmentId);
        }

        public async Task<List<Material>> GetAvailableForStudentAsync(int studentId)
        {
            return await _materialRepository.GetAvailableForStudentAsync(studentId);
        }

        public async Task<List<Material>> GetFilteredAsync(
            string teacherId,
            string? searchString = null,
            int? classFilter = null,
            int? assignmentFilter = null,
            MaterialType? typeFilter = null,
            string? sortOrder = null)
        {
            return await _materialRepository.GetFilteredAsync(
                teacherId,
                searchString,
                classFilter,
                assignmentFilter,
                typeFilter,
                sortOrder);
        }

        public async Task<int> CreateAsync(Material material)
        {
            // Валидация: материал должен быть привязан к классу или заданию
            if (!material.ClassId.HasValue && !material.AssignmentId.HasValue)
            {
                throw new ArgumentException("Материал должен быть привязан к классу или заданию");
            }

            // Проверяем доступ учителя к классу/заданию
            if (material.ClassId.HasValue)
            {
                var @class = await _classService.GetByIdAsync(material.ClassId.Value);
                if (@class == null || @class.TeacherId != material.UploadedById)
                {
                    throw new UnauthorizedAccessException("Учитель не имеет доступа к указанному классу");
                }
            }

            if (material.AssignmentId.HasValue)
            {
                var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                if (assignment == null || assignment.TeacherId != material.UploadedById)
                {
                    throw new UnauthorizedAccessException("Учитель не имеет доступа к указанному заданию");
                }
            }

            return await _materialRepository.CreateAsync(material);
        }

        public async Task<int> UpdateAsync(Material material)
        {
            var existingMaterial = await _materialRepository.GetByIdAsync(material.Id);
            if (existingMaterial == null)
            {
                throw new ArgumentException("Материал не найден");
            }

            // Проверяем права доступа
            if (existingMaterial.UploadedById != material.UploadedById)
            {
                throw new UnauthorizedAccessException("Учитель не имеет прав на редактирование этого материала");
            }

            // Валидация: материал должен быть привязан к классу или заданию
            if (!material.ClassId.HasValue && !material.AssignmentId.HasValue)
            {
                throw new ArgumentException("Материал должен быть привязан к классу или заданию");
            }

            // Проверяем доступ учителя к классу/заданию
            if (material.ClassId.HasValue)
            {
                var @class = await _classService.GetByIdAsync(material.ClassId.Value);
                if (@class == null || @class.TeacherId != material.UploadedById)
                {
                    throw new UnauthorizedAccessException("Учитель не имеет доступа к указанному классу");
                }
            }

            if (material.AssignmentId.HasValue)
            {
                var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                if (assignment == null || assignment.TeacherId != material.UploadedById)
                {
                    throw new UnauthorizedAccessException("Учитель не имеет доступа к указанному заданию");
                }
            }

            return await _materialRepository.UpdateAsync(material);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _materialRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            return material != null;
        }

        public async Task<bool> TeacherCanAccessMaterialAsync(string teacherId, int materialId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null)
            {
                return false;
            }

            // Учитель может получить доступ к своим материалам
            if (material.UploadedById == teacherId)
            {
                return true;
            }

            // Или к материалам своих классов
            if (material.ClassId.HasValue)
            {
                var @class = await _classService.GetByIdAsync(material.ClassId.Value);
                if (@class != null && @class.TeacherId == teacherId)
                {
                    return true;
                }
            }

            // Или к материалам своих заданий
            if (material.AssignmentId.HasValue)
            {
                var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                if (assignment != null && assignment.TeacherId == teacherId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

