using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    public class AssignmentTestsViewModel
    {
        public Assignment Assignment { get; set; } = null!;
        public List<SpellingTest> SpellingTests { get; set; } = new();
        public List<PunctuationTest> PunctuationTests { get; set; } = new();
        public List<OrthoeopyTest> OrthoeopyTests { get; set; } = new();
        public List<RegularTest> RegularTests { get; set; } = new();
        public Dictionary<int, int> SpellingTestQuestionCounts { get; set; } = new();
        public Dictionary<int, int> PunctuationTestQuestionCounts { get; set; } = new();
        public Dictionary<int, int> OrthoeopyTestQuestionCounts { get; set; } = new();
        public Dictionary<int, int> RegularTestQuestionCounts { get; set; } = new();

        public int TotalTestsCount => SpellingTests.Count + PunctuationTests.Count + OrthoeopyTests.Count + RegularTests.Count;
    }
}

