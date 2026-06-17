using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.Models{
    [Table("habits")]
    public class Habits:BaseModel, INotifyPropertyChanged{
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PrimaryKey("id", false)] //false bo Supabase sama wygeneruje UUID
        public string Id {get;set;}

        [Column("user_id")]
        public string UserId {get;set;}



        [Column("habit_type_id")]
        public string HabitTypeId{get;set;}

        [Column("name")]
        public string Name{get;set;}

        [Column("icon")]
        public string Icon{get;set;}

        [Column("period")]
        public string Period{get;set;}

        [Column("target_frequency")]
        public int TargetFrequency {get;set;}

        [Column("days_of_week")]
        public int DaysOfWeek{get;set;}

        [Column("priority")]
        public int Priority{get;set;}

        [Column("is_system")]
        public bool IsSystem{get;set;}

        [Column("is_archived")]
        public bool IsArchived{get;set;}

        [Column("created_date")]
        public DateTime CreatedDate {get;set;}
    
        [Column("isFlexible")]
        public bool IsFlexible{get;set;}

        private string? _unit;
        [Column("unit")]
        public string? Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayUnit));
                }
            }
        }

        private int _sortOrder;
        [Column("sort_order")]
        public int SortOrder
        {
            get => _sortOrder;
            set
            {
                if (_sortOrder != value)
                {
                    _sortOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        //Relacje (Foreign Keys)
        [Reference(typeof(Users))]
        public Users User {get;set;}


        [Reference(typeof(HabitTypes))]
        public HabitTypes HabitType {get;set;}

        // ===== Helper Local Properties (Unmapped) =====
        private bool _isCompleted;
        [Newtonsoft.Json.JsonIgnore]
        public bool IsCompleted
        {
            get => _isCompleted;
            set 
            { 
                if (_isCompleted != value)
                {
                    _isCompleted = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(IsGreenHighlighted)); 
                    OnPropertyChanged(nameof(IsPartiallyCompleted)); 
                }
            }
        }

        private double _currentProgress;
        [Newtonsoft.Json.JsonIgnore]
        public double CurrentProgress
        {
            get => _currentProgress;
            set 
            { 
                if (_currentProgress != value)
                {
                    _currentProgress = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(CurrentProgressText));
                    OnPropertyChanged(nameof(IsGreenHighlighted)); 
                    OnPropertyChanged(nameof(IsPartiallyCompleted)); 
                }
            }
        }

        private double _periodProgress;
        [Newtonsoft.Json.JsonIgnore]
        public double PeriodProgress
        {
            get => _periodProgress;
            set
            {
                if (_periodProgress != value)
                {
                    _periodProgress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PeriodProgressText));
                    OnPropertyChanged(nameof(IsGreenHighlighted));
                    OnPropertyChanged(nameof(IsPartiallyCompleted));
                    OnPropertyChanged(nameof(IsPeriodCompleted));
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public string PeriodProgressText => $"{PeriodProgress}/{TargetFrequency} {(TargetFrequency == 1 ? "" : (IsCheckboxType ? "dni" : DisplayUnit))}".Trim();

        [Newtonsoft.Json.JsonIgnore]
        public string CurrentProgressText
        {
            get => _currentProgress == 0 ? "" : _currentProgress.ToString();
            set
            {
                if (double.TryParse(value, out double val))
                {
                    CurrentProgress = val;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    CurrentProgress = 0;
                }
                OnPropertyChanged();
            }
        }

        private string _displayTypeName = "Numeric";
        [Newtonsoft.Json.JsonIgnore]
        public string DisplayTypeName
        {
            get => _displayTypeName;
            set 
            { 
                _displayTypeName = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsCheckboxType));
                OnPropertyChanged(nameof(IsNumericOrTimerType));
                OnPropertyChanged(nameof(IsGreenHighlighted));
                OnPropertyChanged(nameof(PeriodProgressText));
            }
        }

        private string _defaultUnit;
        [Newtonsoft.Json.JsonIgnore]
        public string DefaultUnit
        {
            get => _defaultUnit;
            set 
            { 
                _defaultUnit = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(DisplayUnit)); 
                OnPropertyChanged(nameof(PeriodProgressText));
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public string DisplayUnit => !string.IsNullOrEmpty(Unit) ? Unit : DefaultUnit;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsCheckboxType => string.Equals(DisplayTypeName, "Checkbox", StringComparison.OrdinalIgnoreCase);

        [Newtonsoft.Json.JsonIgnore]
        public bool IsNumericOrTimerType => !IsCheckboxType;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsPeriodCompleted
        {
            get
            {
                if (string.Equals(Period, "daily", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(Period))
                {
                    // For daily, it's just based on today's progress
                    if (IsCheckboxType) return IsCompleted;
                    return CurrentProgress >= TargetFrequency;
                }
                else
                {
                    // For weekly/monthly, based on period progress
                    // But if TargetFrequency is 0 or 1, maybe just simple logic
                    if (TargetFrequency <= 0) return PeriodProgress > 0;
                    return PeriodProgress >= TargetFrequency;
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsPartiallyCompleted
        {
            get
            {
                // Only applicable to weekly/monthly
                if (string.Equals(Period, "daily", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(Period))
                {
                    return false;
                }

                // If fully completed for the period, it's not "partially" completed
                if (IsPeriodCompleted) return false;

                // Otherwise, it's partially completed if it was done *today*
                if (IsCheckboxType) return IsCompleted;
                return CurrentProgress > 0;
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsGreenHighlighted
        {
            get
            {
                return IsPeriodCompleted;
            }
        }
    }
}