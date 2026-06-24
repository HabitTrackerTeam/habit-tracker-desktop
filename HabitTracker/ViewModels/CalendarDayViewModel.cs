using System;
using HabitTracker.Services;

namespace HabitTracker.ViewModels
{
    public class CalendarDayViewModel : ViewModelBase
    {
        /// <summary>
        /// Cached score result from calendar generation, reused in the detail panel
        /// to guarantee the calendar cell and right-side panel always agree.
        /// </summary>
        public DailyScoreCalculator.DailyScoreResult ScoreResult { get; set; }

        private string _dayNumber;
        public string DayNumber
        {
            get => _dayNumber;
            set { _dayNumber = value; OnPropertyChanged(); }
        }

        private string _percentageText;
        public string PercentageText
        {
            get => _percentageText;
            set { _percentageText = value; OnPropertyChanged(); }
        }

        private bool _isCurrentMonth;
        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set { _isCurrentMonth = value; OnPropertyChanged(); }
        }

        private bool _isToday;
        public bool IsToday
        {
            get => _isToday;
            set { _isToday = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private string _badgeColor;
        public string BadgeColor
        {
            get => _badgeColor;
            set { _badgeColor = value; OnPropertyChanged(); }
        }
        
        private string _dotColor;
        public string DotColor
        {
            get => _dotColor;
            set { _dotColor = value; OnPropertyChanged(); }
        }

        public DateTime Date { get; set; }
    }
}
