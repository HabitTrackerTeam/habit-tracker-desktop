using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HabitTracker.Models;
using HabitTracker.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Supabase.Postgrest;

namespace HabitTracker.ViewModels
{
    public class HabitStatisticsViewModel : ViewModelBase
    {
        private string _habitName;
        public string HabitName { get => _habitName; set { _habitName = value; OnPropertyChanged(); } }

        private string _habitIcon;
        public string HabitIcon { get => _habitIcon; set { _habitIcon = value; OnPropertyChanged(); } }

        private string _habitTypeName;
        public string HabitTypeName { get => _habitTypeName; set { _habitTypeName = value; OnPropertyChanged(); } }

        private int _currentStreak;
        public int CurrentStreak { get => _currentStreak; set { _currentStreak = value; OnPropertyChanged(); } }

        private int _longestStreak;
        public int LongestStreak { get => _longestStreak; set { _longestStreak = value; OnPropertyChanged(); } }

        private int _weeklySuccessRate;
        public int WeeklySuccessRate { get => _weeklySuccessRate; set { _weeklySuccessRate = value; OnPropertyChanged(); } }

        private int _totalCompletions;
        public int TotalCompletions { get => _totalCompletions; set { _totalCompletions = value; OnPropertyChanged(); } }

        private bool _isNumericOrTimer;
        public bool IsNumericOrTimer { get => _isNumericOrTimer; set { _isNumericOrTimer = value; OnPropertyChanged(); } }

        // Mini-calendar for the current month
        private ObservableCollection<MiniCalendarDay> _monthlyCompletions = new();
        public ObservableCollection<MiniCalendarDay> MonthlyCompletions
        {
            get => _monthlyCompletions;
            set { _monthlyCompletions = value; OnPropertyChanged(); }
        }

        private string _miniCalendarMonthYear;
        public string MiniCalendarMonthYear { get => _miniCalendarMonthYear; set { _miniCalendarMonthYear = value; OnPropertyChanged(); } }

        // LiveCharts bindings
        private ISeries[] _chartSeries = Array.Empty<ISeries>();
        public ISeries[] ChartSeries { get => _chartSeries; set { _chartSeries = value; OnPropertyChanged(); } }

        private Axis[] _xAxes = Array.Empty<Axis>();
        public Axis[] XAxes { get => _xAxes; set { _xAxes = value; OnPropertyChanged(); } }

        private Axis[] _yAxes = Array.Empty<Axis>();
        public Axis[] YAxes { get => _yAxes; set { _yAxes = value; OnPropertyChanged(); } }

        /// <summary>
        /// Calculates all statistics for a given habit from the database.
        /// </summary>
        public async Task CalculateStatisticsAsync(Habits habit, List<HabitTypes> habitTypes)
        {
            HabitName = habit.Name;
            HabitIcon = habit.Icon;

            // Determine habit type
            var type = habitTypes.FirstOrDefault(t => t.Id == habit.HabitTypeId);
            if (type != null)
            {
                habit.DisplayTypeName = type.DisplayType;
                habit.DefaultUnit = type.DefaultUnit;
            }
            HabitTypeName = type?.DisplayType ?? "Checkbox";
            IsNumericOrTimer = !string.Equals(HabitTypeName, "Checkbox", StringComparison.OrdinalIgnoreCase);

            // Fetch all logs for this habit
            var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                .Filter("habit_id", Constants.Operator.Equals, habit.Id)
                .Order("log_date", Constants.Ordering.Ascending)
                .Get();
            var allLogs = logsResponse.Models ?? new List<HabitLogs>();

            // Build a dictionary of completed dates for fast lookup
            var completedDates = new HashSet<DateTime>();
            foreach (var log in allLogs)
            {
                if (IsHabitCompleted(habit, log))
                {
                    completedDates.Add(log.LogDate.Date);
                }
            }

            TotalCompletions = completedDates.Count;

            // Calculate streaks based on scheduled days only (Option A)
            CalculateStreaks(habit, completedDates);

            // Weekly success rate (last 7 scheduled days)
            CalculateWeeklySuccess(habit, completedDates);

            // Monthly completions mini-calendar
            BuildMiniCalendar(habit, completedDates, DateTime.Now);

            // Chart for numeric/timer habits
            if (IsNumericOrTimer)
            {
                BuildValueTrendChart(allLogs);
            }
            else
            {
                ChartSeries = Array.Empty<ISeries>();
            }
        }

        /// <summary>
        /// Determines if a habit log counts as "completed" based on habit type.
        /// </summary>
        public static bool IsHabitCompleted(Habits habit, HabitLogs log)
        {
            if (log == null) return false;

            // Get display type from habit's helper property
            bool isCheckbox = string.Equals(habit.DisplayTypeName, "Checkbox", StringComparison.OrdinalIgnoreCase);

            if (isCheckbox)
            {
                return log.IsCompleted;
            }
            else
            {
                // Numeric/Timer: completed when value >= target
                return log.NumericValue >= habit.TargetFrequency;
            }
        }

        private void CalculateStreaks(Habits habit, HashSet<DateTime> completedDates)
        {
            // Walk backwards from today through scheduled days
            int currentStreak = 0;
            int longestStreak = 0;
            int tempStreak = 0;
            bool currentStreakBroken = false;

            var today = DateTime.Today;
            var startDate = habit.CreatedDate.Date;

            // Iterate from today backwards to habit creation date
            for (var date = today; date >= startDate; date = date.AddDays(-1))
            {
                if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;

                // Skip future scheduled days that haven't passed yet (shouldn't happen going backwards from today)
                if (completedDates.Contains(date))
                {
                    tempStreak++;
                    if (!currentStreakBroken)
                    {
                        currentStreak = tempStreak;
                    }
                }
                else
                {
                    // For today, if not completed yet, don't break the streak - just skip
                    if (date == today)
                    {
                        continue;
                    }

                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 0;
                    currentStreakBroken = true;
                }
            }
            longestStreak = Math.Max(longestStreak, tempStreak);

            CurrentStreak = currentStreak;
            LongestStreak = longestStreak;
        }

        private void CalculateWeeklySuccess(Habits habit, HashSet<DateTime> completedDates)
        {
            // Count last 7 scheduled days from today backwards
            int scheduledCount = 0;
            int completedCount = 0;
            var date = DateTime.Today;
            var startDate = habit.CreatedDate.Date;

            while (scheduledCount < 7 && date >= startDate)
            {
                if (DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek))
                {
                    scheduledCount++;
                    if (completedDates.Contains(date))
                    {
                        completedCount++;
                    }
                }
                date = date.AddDays(-1);
            }

            WeeklySuccessRate = scheduledCount > 0 ? (int)Math.Round((double)completedCount / scheduledCount * 100) : 0;
        }

        private void BuildMiniCalendar(Habits habit, HashSet<DateTime> completedDates, DateTime referenceDate)
        {
            MiniCalendarMonthYear = referenceDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("en-US"));

            var firstDay = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);

            int startDayOfWeek = (int)firstDay.DayOfWeek;
            if (startDayOfWeek == 0) startDayOfWeek = 7;

            var days = new ObservableCollection<MiniCalendarDay>();

            // Add empty slots for padding
            for (int i = 1; i < startDayOfWeek; i++)
            {
                days.Add(new MiniCalendarDay { DayNumber = "", IsEmpty = true });
            }

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(referenceDate.Year, referenceDate.Month, d);
                bool isScheduled = DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek) && date >= habit.CreatedDate.Date;
                bool isCompleted = completedDates.Contains(date);
                bool isFuture = date > DateTime.Today;

                string dotColor;
                if (isFuture || !isScheduled)
                {
                    dotColor = "#E5E7EB"; // Gray - not scheduled or future
                }
                else if (isCompleted)
                {
                    dotColor = "#059669"; // Green - completed
                }
                else
                {
                    dotColor = "#EF4444"; // Red - missed
                }

                days.Add(new MiniCalendarDay
                {
                    DayNumber = d.ToString(),
                    IsCompleted = isCompleted,
                    IsScheduled = isScheduled && !isFuture,
                    DotColor = dotColor,
                    IsEmpty = false
                });
            }

            MonthlyCompletions = days;
        }

        private void BuildValueTrendChart(List<HabitLogs> allLogs)
        {
            var sortedLogs = allLogs.OrderBy(l => l.LogDate).ToList();

            if (!sortedLogs.Any())
            {
                ChartSeries = Array.Empty<ISeries>();
                return;
            }

            var values = sortedLogs.Select(l => l.NumericValue).ToList();
            var labels = sortedLogs.Select(l => l.LogDate.ToString("dd MMM", CultureInfo.InvariantCulture)).ToList();

            ChartSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = values,
                    Name = "Value",
                    Fill = new SolidColorPaint(new SKColor(50, 138, 93).WithAlpha(50)),
                    Stroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255)),
                    GeometryStroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 2 }
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 11
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 3, 3 })
                    }
                }
            };
        }
    }

    /// <summary>
    /// Represents a single day in the mini-calendar of the statistics modal.
    /// </summary>
    public class MiniCalendarDay
    {
        public string DayNumber { get; set; } = "";
        public bool IsCompleted { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsEmpty { get; set; }
        public string DotColor { get; set; } = "#E5E7EB";
    }
}
