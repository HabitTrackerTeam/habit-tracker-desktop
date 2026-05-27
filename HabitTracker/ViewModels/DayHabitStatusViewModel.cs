namespace HabitTracker.ViewModels
{
    /// <summary>
    /// Represents a single habit's completion status for a specific day,
    /// displayed in the Calendar right-side detail panel.
    /// </summary>
    public class DayHabitStatusViewModel : ViewModelBase
    {
        private string _habitName;
        public string HabitName
        {
            get => _habitName;
            set { _habitName = value; OnPropertyChanged(); }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Badge text shown on the right side of each habit row.
        /// For checkbox habits: "DONE" or "MISSED"
        /// For numeric/timer habits: formatted value like "8 GLASSES" or "15 MIN"
        /// </summary>
        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The habit type: "checkbox", "numeric", or "timer"
        /// </summary>
        private string _habitType;
        public string HabitType
        {
            get => _habitType;
            set { _habitType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The underlying habit ID (for potential future navigation)
        /// </summary>
        public string HabitId { get; set; }
    }
}
