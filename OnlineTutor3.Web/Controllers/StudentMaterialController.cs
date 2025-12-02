using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class StudentMaterialController : Controller
    {
        private readonly IMaterialService _materialService;
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<StudentMaterialController> _logger;

        public StudentMaterialController(
            IMaterialService materialService,
            IStudentService studentService,
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<StudentMaterialController> logger)
        {
            _materialService = materialService;
            _studentService = studentService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: StudentMaterial
        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var student = await _studentService.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Профиль студента не найден. Обратитесь к администратору.";
                    return RedirectToAction("Index", "Student");
                }

                // Получаем доступные материалы
                var materials = await _materialService.GetAvailableForStudentAsync(student.Id);

                // Применяем поиск
                if (!string.IsNullOrEmpty(search))
                {
                    materials = materials.Where(m =>
                        m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(m.Description) && m.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.FileName) && m.FileName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // Группируем материалы по заданиям
                var assignmentsDict = new Dictionary<int, Assignment>();
                var materialsByAssignment = new Dictionary<int, List<Material>>();

                foreach (var material in materials)
                {
                    if (material.AssignmentId.HasValue)
                    {
                        if (!assignmentsDict.ContainsKey(material.AssignmentId.Value))
                        {
                            var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                            if (assignment != null)
                            {
                                assignmentsDict[material.AssignmentId.Value] = assignment;
                            }
                        }

                        if (!materialsByAssignment.ContainsKey(material.AssignmentId.Value))
                        {
                            materialsByAssignment[material.AssignmentId.Value] = new List<Material>();
                        }
                        materialsByAssignment[material.AssignmentId.Value].Add(material);
                    }
                }

                // Материалы без задания (привязанные только к классу)
                var materialsWithoutAssignment = materials.Where(m => !m.AssignmentId.HasValue).ToList();

                // Загружаем предметы для отображения
                var allSubjects = await _subjectService.GetAllAsync();
                var subjectsDict = allSubjects.ToDictionary(s => s.Id, s => s.Name);

                var viewModel = new StudentMaterialIndexViewModel
                {
                    Student = student,
                    MaterialsByAssignment = materialsByAssignment,
                    AssignmentsDict = assignmentsDict,
                    MaterialsWithoutAssignment = materialsWithoutAssignment,
                    SubjectsDict = subjectsDict,
                    SearchQuery = search
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке материалов для студента");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке материалов. Попробуйте обновить страницу.";
                return RedirectToAction("Index", "Student");
            }
        }

        // GET: StudentMaterial/Download/5
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var student = await _studentService.GetByUserIdAsync(currentUser.Id);
                if (student == null)
                {
                    return Forbid();
                }

                // Получаем доступные материалы студента
                var availableMaterials = await _materialService.GetAvailableForStudentAsync(student.Id);
                var material = availableMaterials.FirstOrDefault(m => m.Id == id);

                if (material == null)
                {
                    _logger.LogWarning("Студент {StudentId} попытался скачать недоступный материал {MaterialId}", student.Id, id);
                    TempData["ErrorMessage"] = "Материал не найден или недоступен.";
                    return RedirectToAction(nameof(Index));
                }

                var filePath = Path.Combine(_environment.WebRootPath, material.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("Файл материала {MaterialId} не найден на сервере. Путь: {FilePath}", id, filePath);
                    TempData["ErrorMessage"] = "Файл не найден на сервере.";
                    return RedirectToAction(nameof(Index));
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                return File(fileBytes, material.ContentType ?? "application/octet-stream", material.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при скачивании материала {MaterialId} студентом", id);
                TempData["ErrorMessage"] = "Произошла ошибка при скачивании файла.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

