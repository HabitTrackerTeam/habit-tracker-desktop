using HabitTracker.Models;

namespace HabitTracker.ViewModels
{
    /// <summary>
    /// Represents a single habit's performance summary displayed in the
    /// Statistics tab's "Detailed Performance" table.
    /// </summary>
    public class HabitPerformanceViewModel : ViewModelBase
    {
        /// <summary>
        /// The underlying Habits model (used to open the detail modal).
        /// </summary>
        public Habits Habit { get; set; }

        private string _habitName;
        public string HabitName
        {
            get => _habitName;
            set { _habitName = value; OnPropertyChanged(); }
        }

        private string _habitDescription;
        public string HabitDescription
        {
            get => _habitDescription;
            set { _habitDescription = value; OnPropertyChanged(); }
        }

        private int _consistency;
        public int Consistency
        {
            get => _consistency;
            set { _consistency = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConsistencyText)); }
        }

        public string ConsistencyText => $"{Consistency}%";

        private string _trendText;
        public string TrendText
        {
            get => _trendText;
            set { _trendText = value; OnPropertyChanged(); }
        }

        private bool _isPositiveTrend;
        public bool IsPositiveTrend
        {
            get => _isPositiveTrend;
            set { _isPositiveTrend = value; OnPropertyChanged(); }
        }

        private int _currentStreak;
        public int CurrentStreak
        {
            get => _currentStreak;
            set { _currentStreak = value; OnPropertyChanged(); OnPropertyChanged(nameof(StreakText)); }
        }

        public string StreakText => $"{CurrentStreak}d";

        /// <summary>
        /// "checkbox", "numeric", or "timer"
        /// </summary>
        private string _habitTypeName;
        public string HabitTypeName
        {
            get => _habitTypeName;
            set { _habitTypeName = value; OnPropertyChanged(); }
        }

        private string _habitIcon = "🌱";
        public string HabitIcon
        {
            get => _habitIcon;
            set { _habitIcon = value; OnPropertyChanged(); }
        }
    }
}
