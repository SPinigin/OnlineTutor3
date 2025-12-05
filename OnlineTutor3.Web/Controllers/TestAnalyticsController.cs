using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTutor3.Application.Interfaces;
using OnlineTutor3.Domain.Entities;
using OnlineTutor3.Web.ViewModels;

namespace OnlineTutor3.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Teacher)]
    public class TestAnalyticsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TestAnalyticsController> _logger;
        private readonly ISpellingTestRepository _spellingTestRepository;
        private readonly IRegularTestRepository _regularTestRepository;
        private readonly IPunctuationTestRepository _punctuationTestRepository;
        private readonly IOrthoeopyTestRepository _orthoeopyTestRepository;
        private readonly ISpellingQuestionRepository _spellingQuestionRepository;
        private readonly IRegularQuestionRepository _regularQuestionRepository;
        private readonly IRegularQuestionOptionRepository _regularQuestionOptionRepository;
        private readonly IPunctuationQuestionRepository _punctuationQuestionRepository;
        private readonly IOrthoeopyQuestionRepository _orthoeopyQuestionRepository;
        private readonly ISpellingTestResultRepository _spellingTestResultRepository;
        private readonly IRegularTestResultRepository _regularTestResultRepository;
        private readonly IPunctuationTestResultRepository _punctuationTestResultRepository;
        private readonly IOrthoeopyTestResultRepository _orthoeopyTestResultRepository;
        private readonly ISpellingAnswerRepository _spellingAnswerRepository;
        private readonly IRegularAnswerRepository _regularAnswerRepository;
        private readonly IPunctuationAnswerRepository _punctuationAnswerRepository;
        private readonly IOrthoeopyAnswerRepository _orthoeopyAnswerRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentClassRepository _assignmentClassRepository;
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _studentRepository;

        public TestAnalyticsController(
            UserManager<ApplicationUser> userManager,
            ILogger<TestAnalyticsController> logger,
            ISpellingTestRepository spellingTestRepository,
            IRegularTestRepository regularTestRepository,
            IPunctuationTestRepository punctuationTestRepository,
            IOrthoeopyTestRepository orthoeopyTestRepository,
            ISpellingQuestionRepository spellingQuestionRepository,
            IRegularQuestionRepository regularQuestionRepository,
            IRegularQuestionOptionRepository regularQuestionOptionRepository,
            IPunctuationQuestionRepository punctuationQuestionRepository,
            IOrthoeopyQuestionRepository orthoeopyQuestionRepository,
            ISpellingTestResultRepository spellingTestResultRepository,
            IRegularTestResultRepository regularTestResultRepository,
            IPunctuationTestResultRepository punctuationTestResultRepository,
            IOrthoeopyTestResultRepository orthoeopyTestResultRepository,
            ISpellingAnswerRepository spellingAnswerRepository,
            IRegularAnswerRepository regularAnswerRepository,
            IPunctuationAnswerRepository punctuationAnswerRepository,
            IOrthoeopyAnswerRepository orthoeopyAnswerRepository,
            IAssignmentRepository assignmentRepository,
            IAssignmentClassRepository assignmentClassRepository,
            IClassRepository classRepository,
            IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _logger = logger;
            _spellingTestRepository = spellingTestRepository;
            _regularTestRepository = regularTestRepository;
            _punctuationTestRepository = punctuationTestRepository;
            _orthoeopyTestRepository = orthoeopyTestRepository;
            _spellingQuestionRepository = spellingQuestionRepository;
            _regularQuestionRepository = regularQuestionRepository;
            _regularQuestionOptionRepository = regularQuestionOptionRepository;
            _punctuationQuestionRepository = punctuationQuestionRepository;
            _orthoeopyQuestionRepository = orthoeopyQuestionRepository;
            _spellingTestResultRepository = spellingTestResultRepository;
            _regularTestResultRepository = regularTestResultRepository;
            _punctuationTestResultRepository = punctuationTestResultRepository;
            _orthoeopyTestResultRepository = orthoeopyTestResultRepository;
            _spellingAnswerRepository = spellingAnswerRepository;
            _regularAnswerRepository = regularAnswerRepository;
            _punctuationAnswerRepository = punctuationAnswerRepository;
            _orthoeopyAnswerRepository = orthoeopyAnswerRepository;
            _assignmentRepository = assignmentRepository;
            _assignmentClassRepository = assignmentClassRepository;
            _classRepository = classRepository;
            _studentRepository = studentRepository;
        }

        // GET: TestAnalytics/Spelling/5 - Аналитика теста по орфографии
        public async Task<IActionResult> Spelling(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var test = await _spellingTestRepository.GetByIdAsync(id);
            
            if (test == null || test.TeacherId != currentUser.Id) return NotFound();

            var analytics = await BuildSpellingAnalyticsAsync(test);
            
            // Загружаем пользователей для студентов
            foreach (var result in analytics.SpellingResults)
            {
                result.Student.User = await _userManager.FindByIdAsync(result.Student.UserId);
            }
            
            foreach (var student in analytics.StudentsNotTaken)
            {
                student.User = await _userManager.FindByIdAsync(student.UserId);
            }
            
            return View("SpellingAnalytics", analytics);
        }

        // GET: TestAnalytics/Regular/1 - Аналитика обычного теста
        public async Task<IActionResult> Regular(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var test = await _regularTestRepository.GetByIdAsync(id);
            
            if (test == null || test.TeacherId != currentUser.Id) return NotFound();

            var analytics = await BuildRegularTestAnalyticsAsync(test);
            
            // Загружаем пользователей для студентов
            foreach (var result in analytics.RegularResults)
            {
                result.Student.User = await _userManager.FindByIdAsync(result.Student.UserId);
            }
            
            foreach (var student in analytics.StudentsNotTaken)
            {
                student.User = await _userManager.FindByIdAsync(student.UserId);
            }
            
            return View("RegularTestAnalytics", analytics);
        }

        // GET: TestAnalytics/Orthoeopy/6 - Аналитика теста орфоэпии
        public async Task<IActionResult> Orthoeopy(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var test = await _orthoeopyTestRepository.GetByIdAsync(id);
            
            if (test == null || test.TeacherId != currentUser.Id) return NotFound();

            var analytics = await BuildOrthoeopyAnalyticsAsync(test);
            
            // Загружаем пользователей для студентов
            foreach (var result in analytics.StudentResults)
            {
                result.Student.User = await _userManager.FindByIdAsync(result.Student.UserId);
            }
            
            foreach (var student in analytics.StudentsNotTaken)
            {
                student.User = await _userManager.FindByIdAsync(student.UserId);
            }
            
            return View("OrthoeopyAnalytics", analytics);
        }

        // GET: TestAnalytics/Punctuation/5 - Аналитика теста по пунктуации
        public async Task<IActionResult> Punctuation(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var test = await _punctuationTestRepository.GetByIdAsync(id);
            
            if (test == null || test.TeacherId != currentUser.Id) return NotFound();

            var analytics = await BuildPunctuationAnalyticsAsync(test);
            
            // Загружаем пользователей для студентов
            foreach (var result in analytics.StudentResults)
            {
                result.Student.User = await _userManager.FindByIdAsync(result.Student.UserId);
            }
            
            foreach (var student in analytics.StudentsNotTaken)
            {
                student.User = await _userManager.FindByIdAsync(student.UserId);
            }
            
            return View("PunctuationAnalytics", analytics);
        }

        // Вспомогательный метод для получения студентов из классов, назначенных заданию
        private async Task<List<Student>> GetStudentsFromAssignmentAsync(int assignmentId)
        {
            var allStudents = new List<Student>();
            
            // Получаем классы, назначенные заданию
            var assignmentClasses = await _assignmentClassRepository.GetByAssignmentIdAsync(assignmentId);
            
            if (assignmentClasses.Any())
            {
                // Получаем студентов из всех назначенных классов
                var classIds = assignmentClasses.Select(ac => ac.ClassId).ToList();
                
                foreach (var classId in classIds)
                {
                    var students = await _studentRepository.GetByClassIdAsync(classId);
                    allStudents.AddRange(students);
                }
                
                // Удаляем дубликаты
                allStudents = allStudents.GroupBy(s => s.Id).Select(g => g.First()).ToList();
            }
            else
            {
                // Если задание не назначено классам, возвращаем пустой список
                // В новом проекте все задания должны быть назначены классам
            }
            
            return allStudents;
        }

        // Методы для построения аналитики будут добавлены ниже
        // (Из-за ограничения размера ответа, они будут добавлены в следующем шаге)
        
        private async Task<SpellingTestAnalyticsViewModel> BuildSpellingAnalyticsAsync(SpellingTest test)
        {
            var analytics = new SpellingTestAnalyticsViewModel
            {
                SpellingTest = test
            };

            // Получаем студентов из классов, назначенных заданию
            var allStudents = await GetStudentsFromAssignmentAsync(test.AssignmentId);

            // Получаем вопросы теста
            var questions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(test.Id);

            // Получаем все результаты теста
            var allResults = await _spellingTestResultRepository.GetByTestIdAsync(test.Id);

            // Строим статистику
            analytics.Statistics = await BuildSpellingStatisticsAsync(test, allStudents, allResults);

            // Строим результаты студентов
            analytics.SpellingResults = await BuildSpellingStudentResultsAsync(test, allStudents);

            // Строим аналитику вопросов
            analytics.SpellingQuestionAnalytics = await BuildSpellingQuestionAnalyticsAsync(test, questions);

            // Студенты, которые не проходили тест
            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            analytics.StudentsNotTaken = allStudents.Where(s => !studentsWithResults.Contains(s.Id)).ToList();

            return analytics;
        }

        private async Task<SpellingTestStatistics> BuildSpellingStatisticsAsync(SpellingTest test, List<Student> allStudents, List<SpellingTestResult> allResults)
        {
            var stats = new SpellingTestStatistics
            {
                TotalStudents = allStudents.Count
            };

            var completedResults = allResults.Where(r => r.IsCompleted).ToList();
            var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
            
            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsCompleted = completedResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsInProgress = inProgressResults.Select(r => r.StudentId).Distinct().ToHashSet();

            stats.StudentsCompleted = studentsCompleted.Count;
            stats.StudentsInProgress = studentsInProgress.Count;
            stats.StudentsNotStarted = allStudents.Count - studentsWithResults.Count;

            if (completedResults.Any())
            {
                stats.AverageScore = Math.Round(completedResults.Average(r => r.Score), 1);
                stats.AveragePercentage = Math.Round(completedResults.Average(r => r.Percentage), 1);
                stats.HighestScore = completedResults.Max(r => r.Score);
                stats.LowestScore = completedResults.Min(r => r.Score);
                var completedResultsWithDate = completedResults.Where(r => r.CompletedAt.HasValue).ToList();
                if (completedResultsWithDate.Any())
                {
                    stats.FirstCompletion = completedResultsWithDate.Min(r => r.CompletedAt);
                    stats.LastCompletion = completedResultsWithDate.Max(r => r.CompletedAt);
                }

                // Среднее время выполнения
                var completionTimes = completedResults
                    .Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds)
                    .Where(seconds => seconds > 0)
                    .ToList();
                
                if (completionTimes.Any())
                {
                    stats.AverageCompletionTime = TimeSpan.FromSeconds(completionTimes.Average());
                }

                // Распределение оценок
                stats.GradeDistribution = new Dictionary<string, int>();
                foreach (var result in completedResults.Where(r => r.Grade.HasValue))
                {
                    var gradeKey = GetGradeName(result.Grade!.Value);
                    if (!stats.GradeDistribution.ContainsKey(gradeKey))
                    {
                        stats.GradeDistribution[gradeKey] = 0;
                    }
                    stats.GradeDistribution[gradeKey]++;
                }
            }

            return stats;
        }

        private async Task<List<SpellingStudentResultViewModel>> BuildSpellingStudentResultsAsync(SpellingTest test, List<Student> allStudents)
        {
            var studentResults = new List<SpellingStudentResultViewModel>();

            foreach (var student in allStudents)
            {
                var results = await _spellingTestResultRepository.GetByStudentAndTestIdAsync(student.Id, test.Id);
                var completedResults = results.Where(r => r.IsCompleted).ToList();

                var studentResult = new SpellingStudentResultViewModel
                {
                    Student = student,
                    Results = results,
                    AttemptsUsed = results.Count,
                    HasCompleted = completedResults.Any(),
                    IsInProgress = results.Any(r => !r.IsCompleted)
                };

                if (completedResults.Any())
                {
                    studentResult.BestResult = completedResults.OrderByDescending(r => r.Percentage).First();
                    studentResult.LatestResult = completedResults.OrderByDescending(r => r.CompletedAt).First();

                    // Общее время
                    var totalSeconds = completedResults
                        .Where(r => r.CompletedAt.HasValue)
                        .Sum(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds);
                    
                    if (totalSeconds > 0)
                    {
                        studentResult.TotalTimeSpent = TimeSpan.FromSeconds(totalSeconds);
                    }
                }

                studentResults.Add(studentResult);
            }

            // Сортируем по фамилии
            var userCache = new Dictionary<string, ApplicationUser?>();
            foreach (var result in studentResults)
            {
                if (!userCache.ContainsKey(result.Student.UserId))
                {
                    userCache[result.Student.UserId] = await _userManager.FindByIdAsync(result.Student.UserId);
                }
            }

            studentResults.Sort((a, b) =>
            {
                var userA = userCache.GetValueOrDefault(a.Student.UserId);
                var userB = userCache.GetValueOrDefault(b.Student.UserId);
                var lastNameA = userA?.LastName ?? "";
                var lastNameB = userB?.LastName ?? "";
                return string.Compare(lastNameA, lastNameB, StringComparison.Ordinal);
            });

            return studentResults;
        }

        private async Task<List<SpellingQuestionAnalyticsViewModel>> BuildSpellingQuestionAnalyticsAsync(SpellingTest test, List<SpellingQuestion> questions)
        {
            var questionAnalytics = new List<SpellingQuestionAnalyticsViewModel>();

            // Получаем все результаты теста для быстрого доступа
            var allTestResults = await _spellingTestResultRepository.GetByTestIdAsync(test.Id);
            var testResultDict = allTestResults.ToDictionary(r => r.Id);

            // Получаем всех студентов и их пользователей для быстрого доступа
            var studentIds = allTestResults.Select(r => r.StudentId).Distinct().ToList();
            var studentDict = new Dictionary<int, Student>();
            var userDict = new Dictionary<string, ApplicationUser?>();

            foreach (var studentId in studentIds)
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null)
                {
                    studentDict[studentId] = student;
                    if (!userDict.ContainsKey(student.UserId))
                    {
                        userDict[student.UserId] = await _userManager.FindByIdAsync(student.UserId);
                    }
                }
            }

            foreach (var question in questions)
            {
                var answers = await _spellingAnswerRepository.GetByQuestionIdAsync(question.Id);
                
                var analytics = new SpellingQuestionAnalyticsViewModel
                {
                    SpellingQuestion = question,
                    TotalAnswers = answers.Count,
                    CorrectAnswers = answers.Count(a => a.IsCorrect),
                    IncorrectAnswers = answers.Count(a => !a.IsCorrect)
                };

                if (analytics.TotalAnswers > 0)
                {
                    analytics.SuccessRate = Math.Round((double)analytics.CorrectAnswers / analytics.TotalAnswers * 100, 1);

                    // Получаем частые ошибки с именами студентов
                    var mistakeGroups = answers
                        .Where(a => !a.IsCorrect)
                        .GroupBy(a => a.StudentAnswer)
                        .Select(g => new 
                        { 
                            IncorrectAnswer = g.Key, 
                            Answers = g.ToList(),
                            Count = g.Count() 
                        })
                        .OrderByDescending(m => m.Count)
                        .Take(5)
                        .ToList();

                    var incorrectCount = analytics.IncorrectAnswers;

                    analytics.CommonMistakes = mistakeGroups.Select(m => 
                    {
                        // Получаем имена студентов, допустивших эту ошибку
                        var studentNames = new List<string>();
                        var uniqueStudentIds = new HashSet<int>();

                        foreach (var answer in m.Answers)
                        {
                            if (testResultDict.TryGetValue(answer.TestResultId, out var testResult))
                            {
                                if (uniqueStudentIds.Add(testResult.StudentId))
                                {
                                    if (studentDict.TryGetValue(testResult.StudentId, out var student))
                                    {
                                        if (userDict.TryGetValue(student.UserId, out var user) && user != null)
                                        {
                                            studentNames.Add(user.FullName);
                                        }
                                    }
                                }
                            }
                        }

                        return new CommonMistakeViewModel
                        {
                            IncorrectAnswer = m.IncorrectAnswer,
                            Count = m.Count,
                            Percentage = incorrectCount > 0 ? Math.Round((double)m.Count / incorrectCount * 100, 1) : 0,
                            StudentNames = studentNames.OrderBy(n => n).ToList()
                        };
                    }).ToList();
                }

                questionAnalytics.Add(analytics);
            }

            // Определяем самые сложные и простые вопросы
            if (questionAnalytics.Any(qa => qa.TotalAnswers > 0))
            {
                var questionsWithAnswers = questionAnalytics.Where(qa => qa.TotalAnswers > 0).ToList();
                var lowestSuccessRate = questionsWithAnswers.Min(qa => qa.SuccessRate);
                var highestSuccessRate = questionsWithAnswers.Max(qa => qa.SuccessRate);

                foreach (var qa in questionAnalytics)
                {
                    if (qa.TotalAnswers > 0)
                    {
                        qa.IsMostDifficult = qa.SuccessRate == lowestSuccessRate;
                        qa.IsEasiest = qa.SuccessRate == highestSuccessRate;
                    }
                }
            }

            return questionAnalytics;
        }

        private string GetGradeName(int grade)
        {
            return grade switch
            {
                5 => "Отлично (5)",
                4 => "Хорошо (4)",
                3 => "Удовлетворительно (3)",
                2 => "Неудовлетворительно (2)",
                _ => "Неудовлетворительно (2)"
            };
        }
        
        private async Task<RegularTestAnalyticsViewModel> BuildRegularTestAnalyticsAsync(RegularTest test)
        {
            var analytics = new RegularTestAnalyticsViewModel
            {
                RegularTest = test
            };

            var allStudents = await GetStudentsFromAssignmentAsync(test.AssignmentId);
            var questions = await _regularQuestionRepository.GetByTestIdOrderedAsync(test.Id);
            var allResults = await _regularTestResultRepository.GetByTestIdAsync(test.Id);

            analytics.Statistics = await BuildRegularTestStatisticsAsync(test, allStudents, allResults);
            analytics.RegularResults = await BuildRegularTestStudentResultsAsync(test, allStudents);
            analytics.QuestionAnalytics = await BuildRegularTestQuestionAnalyticsAsync(test, questions);

            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            analytics.StudentsNotTaken = allStudents.Where(s => !studentsWithResults.Contains(s.Id)).ToList();

            return analytics;
        }

        private async Task<RegularTestStatistics> BuildRegularTestStatisticsAsync(RegularTest test, List<Student> allStudents, List<RegularTestResult> allResults)
        {
            var stats = new RegularTestStatistics { TotalStudents = allStudents.Count };

            var completedResults = allResults.Where(r => r.IsCompleted).ToList();
            var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
            
            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsCompleted = completedResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsInProgress = inProgressResults.Select(r => r.StudentId).Distinct().ToHashSet();

            stats.StudentsCompleted = studentsCompleted.Count;
            stats.StudentsInProgress = studentsInProgress.Count;
            stats.StudentsNotStarted = allStudents.Count - studentsWithResults.Count;

            if (completedResults.Any())
            {
                stats.AverageScore = Math.Round(completedResults.Average(r => r.Score), 1);
                stats.AveragePercentage = Math.Round(completedResults.Average(r => r.Percentage), 1);
                stats.HighestScore = completedResults.Max(r => r.Score);
                stats.LowestScore = completedResults.Min(r => r.Score);
                var completedResultsWithDate = completedResults.Where(r => r.CompletedAt.HasValue).ToList();
                if (completedResultsWithDate.Any())
                {
                    stats.FirstCompletion = completedResultsWithDate.Min(r => r.CompletedAt);
                    stats.LastCompletion = completedResultsWithDate.Max(r => r.CompletedAt);
                }

                var completionTimes = completedResults
                    .Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds)
                    .Where(seconds => seconds > 0)
                    .ToList();
                
                if (completionTimes.Any())
                {
                    stats.AverageCompletionTime = TimeSpan.FromSeconds(completionTimes.Average());
                }

                stats.GradeDistribution = new Dictionary<string, int>();
                foreach (var result in completedResults.Where(r => r.Grade.HasValue))
                {
                    var gradeKey = GetGradeName(result.Grade!.Value);
                    if (!stats.GradeDistribution.ContainsKey(gradeKey))
                    {
                        stats.GradeDistribution[gradeKey] = 0;
                    }
                    stats.GradeDistribution[gradeKey]++;
                }
            }

            return stats;
        }

        private async Task<List<RegularTestStudentResultViewModel>> BuildRegularTestStudentResultsAsync(RegularTest test, List<Student> allStudents)
        {
            var studentResults = new List<RegularTestStudentResultViewModel>();

            foreach (var student in allStudents)
            {
                var results = await _regularTestResultRepository.GetByStudentAndTestIdAsync(student.Id, test.Id);
                var completedResults = results.Where(r => r.IsCompleted).ToList();

                var studentResult = new RegularTestStudentResultViewModel
                {
                    Student = student,
                    Results = results,
                    AttemptsUsed = results.Count,
                    HasCompleted = completedResults.Any(),
                    IsInProgress = results.Any(r => !r.IsCompleted)
                };

                if (completedResults.Any())
                {
                    studentResult.BestResult = completedResults.OrderByDescending(r => r.Percentage).First();
                    studentResult.LatestResult = completedResults.OrderByDescending(r => r.CompletedAt).First();

                    var totalSeconds = completedResults
                        .Where(r => r.CompletedAt.HasValue)
                        .Sum(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds);
                    
                    if (totalSeconds > 0)
                    {
                        studentResult.TotalTimeSpent = TimeSpan.FromSeconds(totalSeconds);
                    }
                }

                studentResults.Add(studentResult);
            }

            var userCache = new Dictionary<string, ApplicationUser?>();
            foreach (var result in studentResults)
            {
                if (!userCache.ContainsKey(result.Student.UserId))
                {
                    userCache[result.Student.UserId] = await _userManager.FindByIdAsync(result.Student.UserId);
                }
            }

            studentResults.Sort((a, b) =>
            {
                var userA = userCache.GetValueOrDefault(a.Student.UserId);
                var userB = userCache.GetValueOrDefault(b.Student.UserId);
                var lastNameA = userA?.LastName ?? "";
                var lastNameB = userB?.LastName ?? "";
                return string.Compare(lastNameA, lastNameB, StringComparison.Ordinal);
            });

            return studentResults;
        }

        private async Task<List<RegularTestQuestionAnalyticsViewModel>> BuildRegularTestQuestionAnalyticsAsync(RegularTest test, List<RegularQuestion> questions)
        {
            var questionAnalytics = new List<RegularTestQuestionAnalyticsViewModel>();

            foreach (var question in questions)
            {
                var answers = await _regularAnswerRepository.GetByQuestionIdAsync(question.Id);
                
                var analytics = new RegularTestQuestionAnalyticsViewModel
                {
                    RegularQuestion = question,
                    TotalAnswers = answers.Count,
                    CorrectAnswers = answers.Count(a => a.IsCorrect),
                    IncorrectAnswers = answers.Count(a => !a.IsCorrect)
                };

                if (analytics.TotalAnswers > 0)
                {
                    analytics.SuccessRate = Math.Round((double)analytics.CorrectAnswers / analytics.TotalAnswers * 100, 1);
                }

                questionAnalytics.Add(analytics);
            }

            if (questionAnalytics.Any(qa => qa.TotalAnswers > 0))
            {
                var questionsWithAnswers = questionAnalytics.Where(qa => qa.TotalAnswers > 0).ToList();
                var lowestSuccessRate = questionsWithAnswers.Min(qa => qa.SuccessRate);
                var highestSuccessRate = questionsWithAnswers.Max(qa => qa.SuccessRate);

                foreach (var qa in questionAnalytics)
                {
                    if (qa.TotalAnswers > 0)
                    {
                        qa.IsMostDifficult = qa.SuccessRate == lowestSuccessRate;
                        qa.IsEasiest = qa.SuccessRate == highestSuccessRate;
                    }
                }
            }

            return questionAnalytics;
        }

        private async Task<PunctuationTestAnalyticsViewModel> BuildPunctuationAnalyticsAsync(PunctuationTest test)
        {
            var analytics = new PunctuationTestAnalyticsViewModel
            {
                PunctuationTest = test
            };

            var allStudents = await GetStudentsFromAssignmentAsync(test.AssignmentId);
            var questions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(test.Id);
            var allResults = await _punctuationTestResultRepository.GetByTestIdAsync(test.Id);

            analytics.Statistics = await BuildPunctuationStatisticsAsync(test, allStudents, allResults);
            analytics.StudentResults = await BuildPunctuationStudentResultsAsync(test, allStudents);
            analytics.QuestionAnalytics = await BuildPunctuationQuestionAnalyticsAsync(test, questions);

            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            analytics.StudentsNotTaken = allStudents.Where(s => !studentsWithResults.Contains(s.Id)).ToList();

            return analytics;
        }

        private async Task<PunctuationTestStatistics> BuildPunctuationStatisticsAsync(PunctuationTest test, List<Student> allStudents, List<PunctuationTestResult> allResults)
        {
            var stats = new PunctuationTestStatistics { TotalStudents = allStudents.Count };

            var completedResults = allResults.Where(r => r.IsCompleted).ToList();
            var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
            
            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsCompleted = completedResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsInProgress = inProgressResults.Select(r => r.StudentId).Distinct().ToHashSet();

            stats.StudentsCompleted = studentsCompleted.Count;
            stats.StudentsInProgress = studentsInProgress.Count;
            stats.StudentsNotStarted = allStudents.Count - studentsWithResults.Count;

            if (completedResults.Any())
            {
                stats.AverageScore = Math.Round(completedResults.Average(r => r.Score), 1);
                stats.AveragePercentage = Math.Round(completedResults.Average(r => r.Percentage), 1);
                stats.HighestScore = completedResults.Max(r => r.Score);
                stats.LowestScore = completedResults.Min(r => r.Score);
                var completedResultsWithDate = completedResults.Where(r => r.CompletedAt.HasValue).ToList();
                if (completedResultsWithDate.Any())
                {
                    stats.FirstCompletion = completedResultsWithDate.Min(r => r.CompletedAt);
                    stats.LastCompletion = completedResultsWithDate.Max(r => r.CompletedAt);
                }

                var completionTimes = completedResults
                    .Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds)
                    .Where(seconds => seconds > 0)
                    .ToList();
                
                if (completionTimes.Any())
                {
                    stats.AverageCompletionTime = TimeSpan.FromSeconds(completionTimes.Average());
                }

                stats.GradeDistribution = new Dictionary<string, int>();
                foreach (var result in completedResults.Where(r => r.Grade.HasValue))
                {
                    var gradeKey = GetGradeName(result.Grade!.Value);
                    if (!stats.GradeDistribution.ContainsKey(gradeKey))
                    {
                        stats.GradeDistribution[gradeKey] = 0;
                    }
                    stats.GradeDistribution[gradeKey]++;
                }
            }

            return stats;
        }

        private async Task<List<PunctuationStudentResultViewModel>> BuildPunctuationStudentResultsAsync(PunctuationTest test, List<Student> allStudents)
        {
            var studentResults = new List<PunctuationStudentResultViewModel>();

            foreach (var student in allStudents)
            {
                var results = await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(student.Id, test.Id);
                var completedResults = results.Where(r => r.IsCompleted).ToList();

                var studentResult = new PunctuationStudentResultViewModel
                {
                    Student = student,
                    Results = results,
                    AttemptsUsed = results.Count,
                    HasCompleted = completedResults.Any(),
                    IsInProgress = results.Any(r => !r.IsCompleted)
                };

                if (completedResults.Any())
                {
                    studentResult.BestResult = completedResults.OrderByDescending(r => r.Percentage).First();
                    studentResult.LatestResult = completedResults.OrderByDescending(r => r.CompletedAt).First();

                    var totalSeconds = completedResults
                        .Where(r => r.CompletedAt.HasValue)
                        .Sum(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds);
                    
                    if (totalSeconds > 0)
                    {
                        studentResult.TotalTimeSpent = TimeSpan.FromSeconds(totalSeconds);
                    }
                }

                studentResults.Add(studentResult);
            }

            var userCache = new Dictionary<string, ApplicationUser?>();
            foreach (var result in studentResults)
            {
                if (!userCache.ContainsKey(result.Student.UserId))
                {
                    userCache[result.Student.UserId] = await _userManager.FindByIdAsync(result.Student.UserId);
                }
            }

            studentResults.Sort((a, b) =>
            {
                var userA = userCache.GetValueOrDefault(a.Student.UserId);
                var userB = userCache.GetValueOrDefault(b.Student.UserId);
                var lastNameA = userA?.LastName ?? "";
                var lastNameB = userB?.LastName ?? "";
                return string.Compare(lastNameA, lastNameB, StringComparison.Ordinal);
            });

            return studentResults;
        }

        private async Task<List<PunctuationQuestionAnalyticsViewModel>> BuildPunctuationQuestionAnalyticsAsync(PunctuationTest test, List<PunctuationQuestion> questions)
        {
            var questionAnalytics = new List<PunctuationQuestionAnalyticsViewModel>();

            // Получаем все результаты теста для быстрого доступа
            var allTestResults = await _punctuationTestResultRepository.GetByTestIdAsync(test.Id);
            var testResultDict = allTestResults.ToDictionary(r => r.Id);

            // Получаем всех студентов и их пользователей для быстрого доступа
            var studentIds = allTestResults.Select(r => r.StudentId).Distinct().ToList();
            var studentDict = new Dictionary<int, Student>();
            var userDict = new Dictionary<string, ApplicationUser?>();

            foreach (var studentId in studentIds)
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null)
                {
                    studentDict[studentId] = student;
                    if (!userDict.ContainsKey(student.UserId))
                    {
                        userDict[student.UserId] = await _userManager.FindByIdAsync(student.UserId);
                    }
                }
            }

            foreach (var question in questions)
            {
                var answers = await _punctuationAnswerRepository.GetByQuestionIdAsync(question.Id);
                
                var analytics = new PunctuationQuestionAnalyticsViewModel
                {
                    PunctuationQuestion = question,
                    TotalAnswers = answers.Count,
                    CorrectAnswers = answers.Count(a => a.IsCorrect),
                    IncorrectAnswers = answers.Count(a => !a.IsCorrect)
                };

                if (analytics.TotalAnswers > 0)
                {
                    analytics.SuccessRate = Math.Round((double)analytics.CorrectAnswers / analytics.TotalAnswers * 100, 1);

                    var mistakeGroups = answers
                        .Where(a => !a.IsCorrect)
                        .GroupBy(a => a.StudentAnswer)
                        .Select(g => new 
                        { 
                            IncorrectAnswer = g.Key, 
                            Answers = g.ToList(),
                            Count = g.Count() 
                        })
                        .OrderByDescending(m => m.Count)
                        .Take(5)
                        .ToList();

                    var incorrectCount = analytics.IncorrectAnswers;
                    analytics.CommonMistakes = mistakeGroups.Select(m => 
                    {
                        var studentNames = new List<string>();
                        var uniqueStudentIds = new HashSet<int>();

                        foreach (var answer in m.Answers)
                        {
                            if (testResultDict.TryGetValue(answer.TestResultId, out var testResult))
                            {
                                if (uniqueStudentIds.Add(testResult.StudentId))
                                {
                                    if (studentDict.TryGetValue(testResult.StudentId, out var student))
                                    {
                                        if (userDict.TryGetValue(student.UserId, out var user) && user != null)
                                        {
                                            studentNames.Add(user.FullName);
                                        }
                                    }
                                }
                            }
                        }

                        return new CommonMistakeViewModel
                        {
                            IncorrectAnswer = m.IncorrectAnswer,
                            Count = m.Count,
                            Percentage = incorrectCount > 0 ? Math.Round((double)m.Count / incorrectCount * 100, 1) : 0,
                            StudentNames = studentNames.OrderBy(n => n).ToList()
                        };
                    }).ToList();
                }

                questionAnalytics.Add(analytics);
            }

            if (questionAnalytics.Any(qa => qa.TotalAnswers > 0))
            {
                var questionsWithAnswers = questionAnalytics.Where(qa => qa.TotalAnswers > 0).ToList();
                var lowestSuccessRate = questionsWithAnswers.Min(qa => qa.SuccessRate);
                var highestSuccessRate = questionsWithAnswers.Max(qa => qa.SuccessRate);

                foreach (var qa in questionAnalytics)
                {
                    if (qa.TotalAnswers > 0)
                    {
                        qa.IsMostDifficult = qa.SuccessRate == lowestSuccessRate;
                        qa.IsEasiest = qa.SuccessRate == highestSuccessRate;
                    }
                }
            }

            return questionAnalytics;
        }

        private async Task<OrthoeopyTestAnalyticsViewModel> BuildOrthoeopyAnalyticsAsync(OrthoeopyTest test)
        {
            var analytics = new OrthoeopyTestAnalyticsViewModel
            {
                OrthoeopyTest = test
            };

            var allStudents = await GetStudentsFromAssignmentAsync(test.AssignmentId);
            var questions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(test.Id);
            var allResults = await _orthoeopyTestResultRepository.GetByTestIdAsync(test.Id);

            analytics.Statistics = await BuildOrthoeopyStatisticsAsync(test, allStudents, allResults);
            analytics.StudentResults = await BuildOrthoeopyStudentResultsAsync(test, allStudents);
            analytics.QuestionAnalytics = await BuildOrthoeopyQuestionAnalyticsAsync(test, questions);

            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            analytics.StudentsNotTaken = allStudents.Where(s => !studentsWithResults.Contains(s.Id)).ToList();

            return analytics;
        }

        private async Task<OrthoeopyTestStatistics> BuildOrthoeopyStatisticsAsync(OrthoeopyTest test, List<Student> allStudents, List<OrthoeopyTestResult> allResults)
        {
            var stats = new OrthoeopyTestStatistics { TotalStudents = allStudents.Count };

            var completedResults = allResults.Where(r => r.IsCompleted).ToList();
            var inProgressResults = allResults.Where(r => !r.IsCompleted).ToList();
            
            var studentsWithResults = allResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsCompleted = completedResults.Select(r => r.StudentId).Distinct().ToHashSet();
            var studentsInProgress = inProgressResults.Select(r => r.StudentId).Distinct().ToHashSet();

            stats.StudentsCompleted = studentsCompleted.Count;
            stats.StudentsInProgress = studentsInProgress.Count;
            stats.StudentsNotStarted = allStudents.Count - studentsWithResults.Count;

            if (completedResults.Any())
            {
                stats.AverageScore = Math.Round(completedResults.Average(r => r.Score), 1);
                stats.AveragePercentage = Math.Round(completedResults.Average(r => r.Percentage), 1);
                stats.HighestScore = completedResults.Max(r => r.Score);
                stats.LowestScore = completedResults.Min(r => r.Score);
                var completedResultsWithDate = completedResults.Where(r => r.CompletedAt.HasValue).ToList();
                if (completedResultsWithDate.Any())
                {
                    stats.FirstCompletion = completedResultsWithDate.Min(r => r.CompletedAt);
                    stats.LastCompletion = completedResultsWithDate.Max(r => r.CompletedAt);
                }

                var completionTimes = completedResults
                    .Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds)
                    .Where(seconds => seconds > 0)
                    .ToList();
                
                if (completionTimes.Any())
                {
                    stats.AverageCompletionTime = TimeSpan.FromSeconds(completionTimes.Average());
                }

                stats.GradeDistribution = new Dictionary<string, int>();
                foreach (var result in completedResults.Where(r => r.Grade.HasValue))
                {
                    var gradeKey = GetGradeName(result.Grade!.Value);
                    if (!stats.GradeDistribution.ContainsKey(gradeKey))
                    {
                        stats.GradeDistribution[gradeKey] = 0;
                    }
                    stats.GradeDistribution[gradeKey]++;
                }
            }

            return stats;
        }

        private async Task<List<OrthoeopyStudentResultViewModel>> BuildOrthoeopyStudentResultsAsync(OrthoeopyTest test, List<Student> allStudents)
        {
            var studentResults = new List<OrthoeopyStudentResultViewModel>();

            foreach (var student in allStudents)
            {
                var results = await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(student.Id, test.Id);
                var completedResults = results.Where(r => r.IsCompleted).ToList();

                var studentResult = new OrthoeopyStudentResultViewModel
                {
                    Student = student,
                    Results = results,
                    AttemptsUsed = results.Count,
                    HasCompleted = completedResults.Any(),
                    IsInProgress = results.Any(r => !r.IsCompleted)
                };

                if (completedResults.Any())
                {
                    studentResult.BestResult = completedResults.OrderByDescending(r => r.Percentage).First();
                    studentResult.LatestResult = completedResults.OrderByDescending(r => r.CompletedAt).First();

                    var totalSeconds = completedResults
                        .Where(r => r.CompletedAt.HasValue)
                        .Sum(r => (r.CompletedAt!.Value - r.StartedAt).TotalSeconds);
                    
                    if (totalSeconds > 0)
                    {
                        studentResult.TotalTimeSpent = TimeSpan.FromSeconds(totalSeconds);
                    }
                }

                studentResults.Add(studentResult);
            }

            var userCache = new Dictionary<string, ApplicationUser?>();
            foreach (var result in studentResults)
            {
                if (!userCache.ContainsKey(result.Student.UserId))
                {
                    userCache[result.Student.UserId] = await _userManager.FindByIdAsync(result.Student.UserId);
                }
            }

            studentResults.Sort((a, b) =>
            {
                var userA = userCache.GetValueOrDefault(a.Student.UserId);
                var userB = userCache.GetValueOrDefault(b.Student.UserId);
                var lastNameA = userA?.LastName ?? "";
                var lastNameB = userB?.LastName ?? "";
                return string.Compare(lastNameA, lastNameB, StringComparison.Ordinal);
            });

            return studentResults;
        }

        private async Task<List<OrthoeopyQuestionAnalyticsViewModel>> BuildOrthoeopyQuestionAnalyticsAsync(OrthoeopyTest test, List<OrthoeopyQuestion> questions)
        {
            var questionAnalytics = new List<OrthoeopyQuestionAnalyticsViewModel>();

            // Получаем все результаты теста для быстрого доступа
            var allTestResults = await _orthoeopyTestResultRepository.GetByTestIdAsync(test.Id);
            var testResultDict = allTestResults.ToDictionary(r => r.Id);

            // Получаем всех студентов и их пользователей для быстрого доступа
            var studentIds = allTestResults.Select(r => r.StudentId).Distinct().ToList();
            var studentDict = new Dictionary<int, Student>();
            var userDict = new Dictionary<string, ApplicationUser?>();

            foreach (var studentId in studentIds)
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null)
                {
                    studentDict[studentId] = student;
                    if (!userDict.ContainsKey(student.UserId))
                    {
                        userDict[student.UserId] = await _userManager.FindByIdAsync(student.UserId);
                    }
                }
            }

            foreach (var question in questions)
            {
                var answers = await _orthoeopyAnswerRepository.GetByQuestionIdAsync(question.Id);
                
                var analytics = new OrthoeopyQuestionAnalyticsViewModel
                {
                    OrthoeopyQuestion = question,
                    TotalAnswers = answers.Count,
                    CorrectAnswers = answers.Count(a => a.IsCorrect),
                    IncorrectAnswers = answers.Count(a => !a.IsCorrect)
                };

                if (analytics.TotalAnswers > 0)
                {
                    analytics.SuccessRate = Math.Round((double)analytics.CorrectAnswers / analytics.TotalAnswers * 100, 1);

                    var mistakeGroups = answers
                        .Where(a => !a.IsCorrect)
                        .GroupBy(a => a.StudentAnswer)
                        .Select(g => new 
                        { 
                            IncorrectPosition = g.Key, 
                            Answers = g.ToList(),
                            Count = g.Count() 
                        })
                        .OrderByDescending(m => m.Count)
                        .Take(5)
                        .ToList();

                    var incorrectCount = analytics.IncorrectAnswers;
                    analytics.CommonMistakes = mistakeGroups.Select(m => 
                    {
                        var studentNames = new List<string>();
                        var uniqueStudentIds = new HashSet<int>();

                        foreach (var answer in m.Answers)
                        {
                            if (testResultDict.TryGetValue(answer.TestResultId, out var testResult))
                            {
                                if (uniqueStudentIds.Add(testResult.StudentId))
                                {
                                    if (studentDict.TryGetValue(testResult.StudentId, out var student))
                                    {
                                        if (userDict.TryGetValue(student.UserId, out var user) && user != null)
                                        {
                                            studentNames.Add(user.FullName);
                                        }
                                    }
                                }
                            }
                        }

                        return new StressPositionMistakeViewModel
                        {
                            IncorrectPosition = m.IncorrectPosition,
                            Count = m.Count,
                            Percentage = incorrectCount > 0 ? Math.Round((double)m.Count / incorrectCount * 100, 1) : 0,
                            StudentNames = studentNames.OrderBy(n => n).ToList()
                        };
                    }).ToList();
                }

                questionAnalytics.Add(analytics);
            }

            if (questionAnalytics.Any(qa => qa.TotalAnswers > 0))
            {
                var questionsWithAnswers = questionAnalytics.Where(qa => qa.TotalAnswers > 0).ToList();
                var lowestSuccessRate = questionsWithAnswers.Min(qa => qa.SuccessRate);
                var highestSuccessRate = questionsWithAnswers.Max(qa => qa.SuccessRate);

                foreach (var qa in questionAnalytics)
                {
                    if (qa.TotalAnswers > 0)
                    {
                        qa.IsMostDifficult = qa.SuccessRate == lowestSuccessRate;
                        qa.IsEasiest = qa.SuccessRate == highestSuccessRate;
                    }
                }
            }

            return questionAnalytics;
        }

        // GET: TestAnalytics/StudentDetails/Spelling?testId=1&studentId=1
        public async Task<IActionResult> StudentDetails(string testType, int testId, int studentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            try
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null) return NotFound();

                var user = await _userManager.FindByIdAsync(student.UserId);
                ViewBag.StudentName = user?.FullName ?? "Неизвестно";
                ViewBag.TestType = testType;

                switch (testType.ToLower())
                {
                    case "spelling":
                        var spellingTest = await _spellingTestRepository.GetByIdAsync(testId);
                        if (spellingTest == null || spellingTest.TeacherId != currentUser.Id) return NotFound();
                        
                        var spellingResults = await _spellingTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                        var spellingQuestions = await _spellingQuestionRepository.GetByTestIdOrderedAsync(testId);
                        var spellingAnswers = new List<SpellingAnswer>();
                        
                        foreach (var result in spellingResults)
                        {
                            var resultAnswers = await _spellingAnswerRepository.GetByTestResultIdAsync(result.Id);
                            spellingAnswers.AddRange(resultAnswers);
                        }

                        ViewBag.Test = spellingTest;
                        ViewBag.Results = spellingResults.OrderByDescending(r => r.StartedAt).ToList();
                        ViewBag.Questions = spellingQuestions;
                        ViewBag.Answers = spellingAnswers;
                        return PartialView("_StudentDetailsSpelling");

                    case "regular":
                        var regularTest = await _regularTestRepository.GetByIdAsync(testId);
                        if (regularTest == null || regularTest.TeacherId != currentUser.Id) return NotFound();
                        
                        var regularResults = await _regularTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                        var regularQuestions = await _regularQuestionRepository.GetByTestIdOrderedAsync(testId);
                        var regularAnswers = new List<RegularAnswer>();
                        
                        foreach (var result in regularResults)
                        {
                            var resultAnswers = await _regularAnswerRepository.GetByTestResultIdAsync(result.Id);
                            regularAnswers.AddRange(resultAnswers);
                        }

                        ViewBag.Test = regularTest;
                        ViewBag.Results = regularResults.OrderByDescending(r => r.StartedAt).ToList();
                        ViewBag.Questions = regularQuestions;
                        ViewBag.Answers = regularAnswers;
                        return PartialView("_StudentDetailsRegular");

                    case "punctuation":
                        var punctuationTest = await _punctuationTestRepository.GetByIdAsync(testId);
                        if (punctuationTest == null || punctuationTest.TeacherId != currentUser.Id) return NotFound();
                        
                        var punctuationResults = await _punctuationTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                        var punctuationQuestions = await _punctuationQuestionRepository.GetByTestIdOrderedAsync(testId);
                        var punctuationAnswers = new List<PunctuationAnswer>();
                        
                        foreach (var result in punctuationResults)
                        {
                            var resultAnswers = await _punctuationAnswerRepository.GetByTestResultIdAsync(result.Id);
                            punctuationAnswers.AddRange(resultAnswers);
                        }

                        ViewBag.Test = punctuationTest;
                        ViewBag.Results = punctuationResults.OrderByDescending(r => r.StartedAt).ToList();
                        ViewBag.Questions = punctuationQuestions;
                        ViewBag.Answers = punctuationAnswers;
                        return PartialView("_StudentDetailsPunctuation");

                    case "orthoeopy":
                        var orthoeopyTest = await _orthoeopyTestRepository.GetByIdAsync(testId);
                        if (orthoeopyTest == null || orthoeopyTest.TeacherId != currentUser.Id) return NotFound();
                        
                        var orthoeopyResults = await _orthoeopyTestResultRepository.GetByStudentAndTestIdAsync(studentId, testId);
                        var orthoeopyQuestions = await _orthoeopyQuestionRepository.GetByTestIdOrderedAsync(testId);
                        var orthoeopyAnswers = new List<OrthoeopyAnswer>();
                        
                        foreach (var result in orthoeopyResults)
                        {
                            var resultAnswers = await _orthoeopyAnswerRepository.GetByTestResultIdAsync(result.Id);
                            orthoeopyAnswers.AddRange(resultAnswers);
                        }

                        ViewBag.Test = orthoeopyTest;
                        ViewBag.Results = orthoeopyResults.OrderByDescending(r => r.StartedAt).ToList();
                        ViewBag.Questions = orthoeopyQuestions;
                        ViewBag.Answers = orthoeopyAnswers;
                        return PartialView("_StudentDetailsOrthoeopy");

                    default:
                        return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении детальной аналитики студента. TestType: {TestType}, TestId: {TestId}, StudentId: {StudentId}", testType, testId, studentId);
                return StatusCode(500, "Произошла ошибка при загрузке данных.");
            }
        }
    }
}

