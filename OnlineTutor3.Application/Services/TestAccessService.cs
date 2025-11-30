using Microsoft.Extensions.Logging;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Сервис для проверки доступа студента к тестам
    /// </summary>
    public class TestAccessService : ITestAccessService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassRepository _classRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentClassRepository _assignmentClassRepository;
        private readonly ISpellingTestRepository _spellingTestRepository;
        private readonly IPunctuationTestRepository _punctuationTestRepository;
        private readonly IOrthoeopyTestRepository _orthoeopyTestRepository;
        private readonly IRegularTestRepository _regularTestRepository;
        private readonly ILogger<TestAccessService> _logger;

        public TestAccessService(
            IStudentRepository studentRepository,
            IClassRepository classRepository,
            IAssignmentRepository assignmentRepository,
            IAssignmentClassRepository assignmentClassRepository,
            ISpellingTestRepository spellingTestRepository,
            IPunctuationTestRepository punctuationTestRepository,
            IOrthoeopyTestRepository orthoeopyTestRepository,
            IRegularTestRepository regularTestRepository,
            ILogger<TestAccessService> logger)
        {
            _studentRepository = studentRepository;
            _classRepository = classRepository;
            _assignmentRepository = assignmentRepository;
            _assignmentClassRepository = assignmentClassRepository;
            _spellingTestRepository = spellingTestRepository;
            _punctuationTestRepository = punctuationTestRepository;
            _orthoeopyTestRepository = orthoeopyTestRepository;
            _regularTestRepository = regularTestRepository;
            _logger = logger;
        }

        public async Task<bool> CanAccessSpellingTestAsync(int studentId, int testId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null) return false;

                var test = await _spellingTestRepository.GetByIdAsync(testId);
                if (test == null || !test.IsActive) return false;

                // Проверка дат
                var now = DateTime.Now;
                if (test.StartDate.HasValue && test.StartDate.Value > now) return false;
                if (test.EndDate.HasValue && test.EndDate.Value < now) return false;

                // Проверка через задание
                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null || !assignment.IsActive) return false;

                // Если студент в классе, проверяем назначение задания классу
                if (student.ClassId.HasValue)
                {
                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа к тесту по орфографии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return false;
            }
        }

        public async Task<bool> CanAccessPunctuationTestAsync(int studentId, int testId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null) return false;

                var test = await _punctuationTestRepository.GetByIdAsync(testId);
                if (test == null || !test.IsActive) return false;

                var now = DateTime.Now;
                if (test.StartDate.HasValue && test.StartDate.Value > now) return false;
                if (test.EndDate.HasValue && test.EndDate.Value < now) return false;

                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null || !assignment.IsActive) return false;

                if (student.ClassId.HasValue)
                {
                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа к тесту по пунктуации. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return false;
            }
        }

        public async Task<bool> CanAccessOrthoeopyTestAsync(int studentId, int testId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null) return false;

                var test = await _orthoeopyTestRepository.GetByIdAsync(testId);
                if (test == null || !test.IsActive) return false;

                var now = DateTime.Now;
                if (test.StartDate.HasValue && test.StartDate.Value > now) return false;
                if (test.EndDate.HasValue && test.EndDate.Value < now) return false;

                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null || !assignment.IsActive) return false;

                if (student.ClassId.HasValue)
                {
                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа к тесту по орфоэпии. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return false;
            }
        }

        public async Task<bool> CanAccessRegularTestAsync(int studentId, int testId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null) return false;

                var test = await _regularTestRepository.GetByIdAsync(testId);
                if (test == null || !test.IsActive) return false;

                var now = DateTime.Now;
                if (test.StartDate.HasValue && test.StartDate.Value > now) return false;
                if (test.EndDate.HasValue && test.EndDate.Value < now) return false;

                var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                if (assignment == null || !assignment.IsActive) return false;

                if (student.ClassId.HasValue)
                {
                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступа к классическому тесту. StudentId: {StudentId}, TestId: {TestId}", studentId, testId);
                return false;
            }
        }

        public async Task<List<SpellingTest>> GetAvailableSpellingTestsAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.ClassId.HasValue) return new List<SpellingTest>();

                var @class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                if (@class == null) return new List<SpellingTest>();

                var allTests = await _spellingTestRepository.GetByTeacherIdAsync(@class.TeacherId);
                var now = DateTime.Now;
                var availableTests = new List<SpellingTest>();

                foreach (var test in allTests.Where(t => t.IsActive))
                {
                    if (test.StartDate.HasValue && test.StartDate.Value > now) continue;
                    if (test.EndDate.HasValue && test.EndDate.Value < now) continue;

                    var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                    if (assignment == null || !assignment.IsActive) continue;

                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        availableTests.Add(test);
                    }
                }

                return availableTests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных тестов по орфографии. StudentId: {StudentId}", studentId);
                return new List<SpellingTest>();
            }
        }

        public async Task<List<PunctuationTest>> GetAvailablePunctuationTestsAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.ClassId.HasValue) return new List<PunctuationTest>();

                var @class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                if (@class == null) return new List<PunctuationTest>();

                var allTests = await _punctuationTestRepository.GetByTeacherIdAsync(@class.TeacherId);
                var now = DateTime.Now;
                var availableTests = new List<PunctuationTest>();

                foreach (var test in allTests.Where(t => t.IsActive))
                {
                    if (test.StartDate.HasValue && test.StartDate.Value > now) continue;
                    if (test.EndDate.HasValue && test.EndDate.Value < now) continue;

                    var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                    if (assignment == null || !assignment.IsActive) continue;

                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        availableTests.Add(test);
                    }
                }

                return availableTests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных тестов по пунктуации. StudentId: {StudentId}", studentId);
                return new List<PunctuationTest>();
            }
        }

        public async Task<List<OrthoeopyTest>> GetAvailableOrthoeopyTestsAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.ClassId.HasValue) return new List<OrthoeopyTest>();

                var @class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                if (@class == null) return new List<OrthoeopyTest>();

                var allTests = await _orthoeopyTestRepository.GetByTeacherIdAsync(@class.TeacherId);
                var now = DateTime.Now;
                var availableTests = new List<OrthoeopyTest>();

                foreach (var test in allTests.Where(t => t.IsActive))
                {
                    if (test.StartDate.HasValue && test.StartDate.Value > now) continue;
                    if (test.EndDate.HasValue && test.EndDate.Value < now) continue;

                    var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                    if (assignment == null || !assignment.IsActive) continue;

                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        availableTests.Add(test);
                    }
                }

                return availableTests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных тестов по орфоэпии. StudentId: {StudentId}", studentId);
                return new List<OrthoeopyTest>();
            }
        }

        public async Task<List<RegularTest>> GetAvailableRegularTestsAsync(int studentId)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.ClassId.HasValue) return new List<RegularTest>();

                var @class = await _classRepository.GetByIdAsync(student.ClassId.Value);
                if (@class == null) return new List<RegularTest>();

                var allTests = await _regularTestRepository.GetByTeacherIdAsync(@class.TeacherId);
                var now = DateTime.Now;
                var availableTests = new List<RegularTest>();

                foreach (var test in allTests.Where(t => t.IsActive))
                {
                    if (test.StartDate.HasValue && test.StartDate.Value > now) continue;
                    if (test.EndDate.HasValue && test.EndDate.Value < now) continue;

                    var assignment = await _assignmentRepository.GetByIdAsync(test.AssignmentId);
                    if (assignment == null || !assignment.IsActive) continue;

                    var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignment.Id);
                    if (assignmentClasses.Any(ac => ac.ClassId == student.ClassId.Value))
                    {
                        availableTests.Add(test);
                    }
                }

                return availableTests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доступных классических тестов. StudentId: {StudentId}", studentId);
                return new List<RegularTest>();
            }
        }
    }
}

