using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class MaterialController : Controller
    {
        private readonly IMaterialService _materialService;
        private readonly IClassService _classService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubjectService _subjectService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MaterialController> _logger;

        // Разрешенные типы файлов и их размеры (в байтах)
        private readonly Dictionary<string, long> _allowedFileTypes = new()
        {
            // Документы
            { ".pdf", 50 * 1024 * 1024 },      // 50 MB
            { ".doc", 25 * 1024 * 1024 },      // 25 MB
            { ".docx", 25 * 1024 * 1024 },     // 25 MB
            { ".txt", 5 * 1024 * 1024 },       // 5 MB
            { ".rtf", 10 * 1024 * 1024 },      // 10 MB
            
            // Презентации
            { ".ppt", 50 * 1024 * 1024 },      // 50 MB
            { ".pptx", 50 * 1024 * 1024 },     // 50 MB
            
            // Таблицы
            { ".xls", 25 * 1024 * 1024 },      // 25 MB
            { ".xlsx", 25 * 1024 * 1024 },     // 25 MB
            { ".csv", 5 * 1024 * 1024 },       // 5 MB
            
            // Изображения
            { ".jpg", 10 * 1024 * 1024 },      // 10 MB
            { ".jpeg", 10 * 1024 * 1024 },     // 10 MB
            { ".png", 10 * 1024 * 1024 },      // 10 MB
            { ".gif", 5 * 1024 * 1024 },       // 5 MB
            { ".bmp", 15 * 1024 * 1024 },      // 15 MB
            
            // Аудио
            { ".mp3", 25 * 1024 * 1024 },      // 25 MB
            { ".wav", 50 * 1024 * 1024 },      // 50 MB
            { ".m4a", 25 * 1024 * 1024 },      // 25 MB
            
            // Видео
            { ".mp4", 200 * 1024 * 1024 },     // 200 MB
            { ".avi", 200 * 1024 * 1024 },     // 200 MB
            { ".mov", 200 * 1024 * 1024 },     // 200 MB
            { ".wmv", 200 * 1024 * 1024 },     // 200 MB
        };

        public MaterialController(
            IMaterialService materialService,
            IClassService classService,
            IAssignmentService assignmentService,
            ISubjectService subjectService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<MaterialController> logger)
        {
            _materialService = materialService;
            _classService = classService;
            _assignmentService = assignmentService;
            _subjectService = subjectService;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: Material
        public async Task<IActionResult> Index(string? searchString, int? classFilter, int? assignmentFilter, string? typeFilter, string? sortOrder)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            ViewBag.CurrentFilter = searchString;
            ViewBag.ClassFilter = classFilter;
            ViewBag.AssignmentFilter = assignmentFilter;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.SizeSortParm = sortOrder == "Size" ? "size_desc" : "Size";

            // Получаем классы для фильтра
            var teacherClasses = await _classService.GetByTeacherIdAsync(currentUser.Id);
            ViewBag.Classes = new SelectList(teacherClasses, "Id", "Name");

            // Получаем задания для фильтра
            var teacherAssignments = await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id);
            ViewBag.Assignments = new SelectList(teacherAssignments, "Id", "Title");

            // Получаем материалы текущего учителя с фильтрацией
            MaterialType? typeFilterEnum = null;
            if (!string.IsNullOrEmpty(typeFilter) && Enum.TryParse<MaterialType>(typeFilter, out var materialType))
            {
                typeFilterEnum = materialType;
            }

            var materialsList = await _materialService.GetFilteredAsync(
                currentUser.Id,
                searchString,
                classFilter,
                assignmentFilter,
                typeFilterEnum,
                sortOrder);

            // Группируем материалы по заданиям (материалы уже отсортированы из GetFilteredAsync)
            var materialsByAssignment = new Dictionary<int, List<Material>>();
            var assignmentsDict = new Dictionary<int, Assignment>();
            var materialsWithoutAssignment = new List<Material>();

            foreach (var material in materialsList)
            {
                if (material.AssignmentId.HasValue)
                {
                    if (!materialsByAssignment.ContainsKey(material.AssignmentId.Value))
                    {
                        materialsByAssignment[material.AssignmentId.Value] = new List<Material>();
                    }
                    materialsByAssignment[material.AssignmentId.Value].Add(material);

                    if (!assignmentsDict.ContainsKey(material.AssignmentId.Value))
                    {
                        var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                        if (assignment != null)
                        {
                            assignmentsDict[material.AssignmentId.Value] = assignment;
                        }
                    }
                }
                else
                {
                    materialsWithoutAssignment.Add(material);
                }
            }

            // Материалы уже отсортированы из GetFilteredAsync, но для уверенности можем отсортировать внутри групп
            // (хотя они должны быть уже в правильном порядке)

            // Загружаем предметы для отображения
            var allSubjects = await _subjectService.GetAllAsync();
            var subjectsDict = allSubjects.ToDictionary(s => s.Id, s => s.Name);

            ViewBag.MaterialsByAssignment = materialsByAssignment;
            ViewBag.AssignmentsDict = assignmentsDict;
            ViewBag.MaterialsWithoutAssignment = materialsWithoutAssignment;
            ViewBag.SubjectsDict = subjectsDict;

            // Добавляем типы материалов для фильтра
            ViewBag.MaterialTypes = Enum.GetValues<MaterialType>()
                .Select(mt => new SelectListItem
                {
                    Value = mt.ToString(),
                    Text = GetMaterialTypeDisplayName(mt)
                })
                .ToList();

            return View(materialsList);
        }

        // GET: Material/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var material = await _materialService.GetByIdAsync(id.Value);
            if (material == null) return NotFound();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id.Value))
            {
                return Forbid();
            }

            // Загружаем название задания, если оно привязано
            if (material.AssignmentId.HasValue)
            {
                var assignment = await _assignmentService.GetByIdAsync(material.AssignmentId.Value);
                if (assignment != null)
                {
                    ViewBag.AssignmentTitle = assignment.Title;
                }
            }

            return View(material);
        }

        // GET: Material/Create
        public async Task<IActionResult> Create()
        {
            await LoadClassesAndAssignments();
            return View();
        }

        // POST: Material/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMaterialViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                // Валидация: должно быть выбрано задание
                if (!model.AssignmentId.HasValue)
                {
                    ModelState.AddModelError("AssignmentId", "Выберите задание для привязки материала");
                    await LoadClassesAndAssignments();
                    return View(model);
                }

                // Валидация файла
                var validationResult = ValidateFile(model.File);
                if (!validationResult.IsValid)
                {

                    ModelState.AddModelError("File", validationResult.ErrorMessage);
                    await LoadClassesAndAssignments();
                    return View(model);
                }

                try
                {
                    // Сохраняем файл
                    var filePath = await SaveFileAsync(model.File);

                    var material = new Material
                    {
                        Title = model.Title,
                        Description = model.Description,
                        FilePath = filePath,
                        FileName = model.File.FileName,
                        FileSize = model.File.Length,
                        ContentType = model.File.ContentType,
                        Type = DetermineMaterialType(model.File.FileName),
                        ClassId = null, // Не привязываем к классу
                        AssignmentId = model.AssignmentId,
                        UploadedById = currentUser.Id,
                        UploadedAt = DateTime.Now,
                        IsActive = model.IsActive
                    };

                    await _materialService.CreateAsync(material);

                    TempData["SuccessMessage"] = $"Материал \"{material.Title}\" успешно загружен!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка загрузки материала учителем {TeacherId}. Title: {Title}, FileName: {FileName}",
                        currentUser.Id, model.Title, model.File?.FileName);
                    ModelState.AddModelError("", "Произошла ошибка при загрузке файла. Попробуйте еще раз.");
                }
            }
            else
            {
            }

            await LoadClassesAndAssignments();
            return View(model);
        }

        // GET: Material/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var material = await _materialService.GetByIdAsync(id.Value);
            if (material == null) return NotFound();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id.Value))
            {
                return Forbid();
            }

            var model = new EditMaterialViewModel
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                ClassId = null, // Не показываем класс
                AssignmentId = material.AssignmentId,
                IsActive = material.IsActive,
                CurrentFileName = material.FileName
            };

            await LoadClassesAndAssignments();
            return View(model);
        }

        // POST: Material/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditMaterialViewModel model)
        {
            if (id != model.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                // Валидация: должно быть выбрано задание
                if (!model.AssignmentId.HasValue)
                {
                    ModelState.AddModelError("AssignmentId", "Выберите задание для привязки материала");
                    await LoadClassesAndAssignments();
                    return View(model);
                }

                try
                {
                    var material = await _materialService.GetByIdAsync(id);
                    if (material == null) return NotFound();

                    // Проверяем права доступа
                    if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id))
                    {
                        return Forbid();
                    }

                    // Обновляем основные поля
                    material.Title = model.Title;
                    material.Description = model.Description;
                    material.ClassId = null; // Убираем привязку к классу
                    material.AssignmentId = model.AssignmentId;
                    material.IsActive = model.IsActive;

                    // Если загружен новый файл
                    if (model.NewFile != null)
                    {
                        var validationResult = ValidateFile(model.NewFile);
                        if (!validationResult.IsValid)
                        {
                            ModelState.AddModelError("NewFile", validationResult.ErrorMessage);
                            await LoadClassesAndAssignments();
                            return View(model);
                        }

                        var oldFileName = material.FileName;
                        DeleteFile(material.FilePath); // Удаляем старый файл

                        // Сохраняем новый файл
                        var newFilePath = await SaveFileAsync(model.NewFile);

                        material.FilePath = newFilePath;
                        material.FileName = model.NewFile.FileName;
                        material.FileSize = model.NewFile.Length;
                        material.ContentType = model.NewFile.ContentType;
                        material.Type = DetermineMaterialType(model.NewFile.FileName);

                    }

                    await _materialService.UpdateAsync(material);

                    TempData["SuccessMessage"] = $"Материал \"{material.Title}\" успешно обновлен!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении материала {MaterialId} учителем {TeacherId}", id, currentUser.Id);
                    ModelState.AddModelError("", "Произошла ошибка при сохранении. Попробуйте еще раз.");
                }
            }
            else
            {
            }

            await LoadClassesAndAssignments();
            return View(model);
        }

        // GET: Material/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var material = await _materialService.GetByIdAsync(id.Value);
            if (material == null) return NotFound();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id.Value))
            {
                return Forbid();
            }

            return View(material);
        }

        // POST: Material/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var material = await _materialService.GetByIdAsync(id);
            if (material == null) return NotFound();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id))
            {
                return Forbid();
            }

            var materialTitle = material.Title;
            var materialFileName = material.FileName;

            try
            {
                DeleteFile(material.FilePath); // Удаляем файл

                // Удаляем запись из БД
                await _materialService.DeleteAsync(id);

                TempData["SuccessMessage"] = $"Материал \"{materialTitle}\" успешно удален!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления материала {MaterialId} учителем {TeacherId}", id, currentUser.Id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении материала.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Material/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id))
            {
                return Forbid();
            }

            var material = await _materialService.GetByIdAsync(id);
            if (material == null) return NotFound();

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

        // POST: Material/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var material = await _materialService.GetByIdAsync(id);
            if (material == null) return NotFound();

            // Проверяем права доступа
            if (!await _materialService.TeacherCanAccessMaterialAsync(currentUser.Id, id))
            {
                return Forbid();
            }

            var oldStatus = material.IsActive;
            material.IsActive = !material.IsActive;
            await _materialService.UpdateAsync(material);

            var status = material.IsActive ? "активирован" : "деактивирован";
            TempData["InfoMessage"] = $"Материал \"{material.Title}\" {status}.";

            return RedirectToAction(nameof(Index));
        }

        #region Private Methods

        private async Task LoadClassesAndAssignments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            var classes = (await _classService.GetByTeacherIdAsync(currentUser.Id))
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Classes = new SelectList(classes, "Id", "Name");

            var assignments = (await _assignmentService.GetByTeacherSubjectsAsync(currentUser.Id))
                .Where(a => a.IsActive)
                .OrderBy(a => a.Title)
                .ToList();
            ViewBag.Assignments = new SelectList(assignments, "Id", "Title");
        }

        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Выберите файл для загрузки");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedFileTypes.ContainsKey(extension))
            {
                var allowedExtensions = string.Join(", ", _allowedFileTypes.Keys);
                return (false, $"Неподдерживаемый тип файла. Разрешены: {allowedExtensions}");
            }

            var maxSize = _allowedFileTypes[extension];
            if (file.Length > maxSize)
            {
                var maxSizeMB = maxSize / (1024 * 1024);
                return (false, $"Размер файла превышает {maxSizeMB} МБ для данного типа");
            }

            return (true, string.Empty);
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/materials/{uniqueFileName}";
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private MaterialType DetermineMaterialType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" or ".doc" or ".docx" or ".txt" or ".rtf" => MaterialType.Document,
                ".ppt" or ".pptx" => MaterialType.Presentation,
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => MaterialType.Image,
                ".mp3" or ".wav" or ".m4a" => MaterialType.Audio,
                ".mp4" or ".avi" or ".mov" or ".wmv" => MaterialType.Video,
                _ => MaterialType.Other
            };
        }

        private string GetMaterialTypeDisplayName(MaterialType type)
        {
            return type switch
            {
                MaterialType.Document => "Документы",
                MaterialType.Presentation => "Презентации",
                MaterialType.Image => "Изображения",
                MaterialType.Audio => "Аудио",
                MaterialType.Video => "Видео",
                MaterialType.Other => "Другое",
                _ => type.ToString()
            };
        }

        #endregion
    }
}

