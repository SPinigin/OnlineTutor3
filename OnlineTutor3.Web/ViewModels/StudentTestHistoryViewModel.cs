using OnlineTutor3.Domain.Entities;

namespace OnlineTutor3.Web.ViewModels
{
    /// <summary>
    /// ViewModel для истории прохождения тестов студента
    /// </summary>
    public class StudentTestHistoryViewModel
    {
        public Student Student { get; set; } = null!;
        public string? CurrentTestType { get; set; }
        
        public List<SpellingTestResult> SpellingResults { get; set; } = new();
        public List<PunctuationTestResult> PunctuationResults { get; set; } = new();
        public List<OrthoeopyTestResult> OrthoeopyResults { get; set; } = new();
        public List<RegularTestResult> RegularResults { get; set; } = new();

        // Статистика
        public int TotalTestsCompleted => SpellingResults.Count + PunctuationResults.Count + 
                                         OrthoeopyResults.Count + RegularResults.Count;
        
        public double AveragePercentage
        {
            get
            {
                var allPercentages = new List<double>();
                allPercentages.AddRange(SpellingResults.Select(r => r.Percentage));
                allPercentages.AddRange(PunctuationResults.Select(r => r.Percentage));
                allPercentages.AddRange(OrthoeopyResults.Select(r => r.Percentage));
                allPercentages.AddRange(RegularResults.Select(r => r.Percentage));
                
                return allPercentages.Any() ? allPercentages.Average() : 0.0;
            }
        }
        
        public double BestPercentage
        {
            get
            {
                var allPercentages = new List<double>();
                allPercentages.AddRange(SpellingResults.Select(r => r.Percentage));
                allPercentages.AddRange(PunctuationResults.Select(r => r.Percentage));
                allPercentages.AddRange(OrthoeopyResults.Select(r => r.Percentage));
                allPercentages.AddRange(RegularResults.Select(r => r.Percentage));
                
                return allPercentages.Any() ? allPercentages.Max() : 0.0;
            }
        }
        
        public int TotalPoints => SpellingResults.Sum(r => r.Score) + 
                                 PunctuationResults.Sum(r => r.Score) + 
                                 OrthoeopyResults.Sum(r => r.Score) + 
                                 RegularResults.Sum(r => r.Score);
    }
}

