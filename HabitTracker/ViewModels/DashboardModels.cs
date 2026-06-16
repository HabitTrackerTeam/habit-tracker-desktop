using System.Windows.Media;

namespace HabitTracker.ViewModels
{
    public class WeeklyCompletionDay
    {
        public string DayName { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public Brush BarColor { get; set; }
        public double BarHeight => MaxValue > 0 ? ((double)Value / MaxValue) * 80 : 0;
        public bool IsFuture { get; set; }
    }

    public class TopHabitViewModel
    {
        public string Name { get; set; }
        public int CompletionPercentage { get; set; }
        public Brush IconBgColor { get; set; }
        public Brush IconFgColor { get; set; }
        public string IconPath { get; set; }
    }

    public class MilestoneViewModel
    {
        public string Title { get; set; }
        public int CurrentValue { get; set; }
        public int TargetValue { get; set; }
        public Brush IconBgColor { get; set; }
        public Brush IconFgColor { get; set; }
        public string IconPath { get; set; }
        public string FormattedProgress => $"{CurrentValue}/{TargetValue}";
    }
}
