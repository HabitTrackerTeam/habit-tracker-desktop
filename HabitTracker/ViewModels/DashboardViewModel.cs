//Plik odpowiedzialny za pobieranie nawyków i podawanie ich dalej
using System;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Windows.Input;
using System.Windows.Threading;
using HabitTracker.Commands;
using System.Collections.Generic;
using Supabase.Postgrest;

namespace HabitTracker.ViewModels{

    // Helper class for filter items
    public class FilterItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DashboardViewModel:ViewModelBase{
        //Lista automatycznie odswiezajaca XAML, gdy pojawia sie w niej nowe dane
        private ObservableCollection<Habits> _habits = new ObservableCollection<Habits>();

        public ObservableCollection<Habits> DailyHabits { get; set; } = new ObservableCollection<Habits>();
        public ObservableCollection<Habits> WeeklyHabits { get; set; } = new ObservableCollection<Habits>();
        public ObservableCollection<Habits> MonthlyHabits { get; set; } = new ObservableCollection<Habits>();

        public ObservableCollection<HabitTypes> HabitTypes {get;set;} = new();
        public ObservableCollection<Colors> Colors {get;set;} = new();
        
        // Filter collections
        public ObservableCollection<FilterItem> Priorities { get; set; } = new();
        public ObservableCollection<FilterItem> Statuses { get; set; } = new();
        public ObservableCollection<FilterItem> Frequencies { get; set; } = new();

        // Selected filters
        private FilterItem _selectedPriority;
        public FilterItem SelectedPriority { get => _selectedPriority; set { _selectedPriority = value; OnPropertyChanged(); FilteredHabits?.Refresh(); } }

        private FilterItem _selectedStatus;
        public FilterItem SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); FilteredHabits?.Refresh(); } }

        private FilterItem _selectedFrequency;
        public FilterItem SelectedFrequency { get => _selectedFrequency; set { _selectedFrequency = value; OnPropertyChanged(); FilteredHabits?.Refresh(); } }

        private ICollectionView _filteredHabits;
        public ICollectionView FilteredHabits
        {
            get => _filteredHabits;
            set { _filteredHabits = value; OnPropertyChanged(); }
        }



        //Pola zbierajace dane z formularza
        private string _newHabitName;
        public string NewHabitName{get=>_newHabitName; set{_newHabitName=value; OnPropertyChanged();}}



        private HabitTypes _selectedType;
        public HabitTypes SelectedType {get=>_selectedType; set{_selectedType = value; OnPropertyChanged();}}

        private Habits _editingHabit;
        public Habits EditingHabit { get => _editingHabit; set { _editingHabit = value; OnPropertyChanged(); } }

        private string _modalTitle = "Add New Habit";
        public string ModalTitle { get => _modalTitle; set { _modalTitle = value; OnPropertyChanged(); } }

        private string _modalButtonText = "Plant Habit";
        public string ModalButtonText { get => _modalButtonText; set { _modalButtonText = value; OnPropertyChanged(); } }

        // ===== Add Habit Modal Fields =====
        private string _newHabitType = "Numeric";
        public string NewHabitType { get => _newHabitType; set { _newHabitType = value; OnPropertyChanged(); } }

        private int _newHabitPriority = 2; // 1=High, 2=Medium, 3=Low
        public int NewHabitPriority { get => _newHabitPriority; set { _newHabitPriority = value; OnPropertyChanged(); } }

        private string _newHabitFrequency = "Daily";
        public string NewHabitFrequency { get => _newHabitFrequency; set { _newHabitFrequency = value; OnPropertyChanged(); } }

        private string _newHabitIcon = "❓";
        public string NewHabitIcon { get => _newHabitIcon; set { _newHabitIcon = value; OnPropertyChanged(); } }

        private int _newHabitDaysOfWeek = 127; // All days bitmask
        public int NewHabitDaysOfWeek { get => _newHabitDaysOfWeek; set { _newHabitDaysOfWeek = value; OnPropertyChanged(); } }

        private double _newHabitGoal = 1;
        public double NewHabitGoal { get => _newHabitGoal; set { _newHabitGoal = value; OnPropertyChanged(); } }

        private string _newHabitUnit = "count";
        public string NewHabitUnit { get => _newHabitUnit; set { _newHabitUnit = value; OnPropertyChanged(); } }



        public ObservableCollection<Habits> Habits{
            get=>_habits;
            set{_habits = value; OnPropertyChanged();}
        }

        // Built-in habits (to be populated from DB by backend data)
        public ObservableCollection<string> BuiltInHabits { get; set; } = new();
        private string _selectedBuiltInHabit;
        public string SelectedBuiltInHabit { get => _selectedBuiltInHabit; set { _selectedBuiltInHabit = value; OnPropertyChanged(); } }

        private bool _isBuiltInMode = true;
        public bool IsBuiltInMode { get => _isBuiltInMode; set { _isBuiltInMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCustomMode)); } }

        public bool IsCustomMode => !_isBuiltInMode;

        private bool _isLoading;
        public bool IsLoading{
            get=>_isLoading;
            set{_isLoading = value; OnPropertyChanged();}
        }
        private bool _isAddFormVisible;
        public bool IsAddFormVisible 
        { 
            get => _isAddFormVisible; 
            set { _isAddFormVisible = value; OnPropertyChanged(); } 
        }

        private bool _isHabitsVisible = false;
        public bool IsHabitsVisible
        {
            get => _isHabitsVisible;
            set { _isHabitsVisible = value; OnPropertyChanged(); }
        }

        private bool _isMeasurementsVisible = false;
        public bool IsMeasurementsVisible
        {
            get => _isMeasurementsVisible;
            set { _isMeasurementsVisible = value; OnPropertyChanged(); }
        }
        private bool _isSettingsVisible = false;
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set { _isSettingsVisible = value; OnPropertyChanged(); }
        }

        private bool _isCalendarVisible = false;
        public bool IsCalendarVisible
        {
            get => _isCalendarVisible;
            set { _isCalendarVisible = value; OnPropertyChanged(); }
        }

        private bool _isStatisticsVisible = false;
        public bool IsStatisticsVisible
        {
            get => _isStatisticsVisible;
            set { _isStatisticsVisible = value; OnPropertyChanged(); }
        }

        private bool _isHomeVisible = true;
        public bool IsHomeVisible
        {
            get => _isHomeVisible;
            set { _isHomeVisible = value; OnPropertyChanged(); }
        }

        private bool _isAddHabitModalOpen = false;
        public bool IsAddHabitModalOpen
        {
            get => _isAddHabitModalOpen;
            set { _isAddHabitModalOpen = value; OnPropertyChanged(); }
        }




        private string _currentMonthYear;
        public string CurrentMonthYear
        {
            get => _currentMonthYear;
            set { _currentMonthYear = value; OnPropertyChanged(); }
        }

        private string _currentProtocolText = "Focusing on \"Mindful Mornings\" protocol";
        public string CurrentProtocolText
        {
            get => _currentProtocolText;
            set { _currentProtocolText = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CalendarDayViewModel> _calendarDays = new ObservableCollection<CalendarDayViewModel>();
        public ObservableCollection<CalendarDayViewModel> CalendarDays
        {
            get => _calendarDays;
            set { _calendarDays = value; OnPropertyChanged(); }
        }

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SelectDayCommand { get; }

        private bool _isMonthlyView = true;
        public bool IsMonthlyView
        {
            get => _isMonthlyView;
            set { _isMonthlyView = value; OnPropertyChanged(); }
        }

        public ICommand SetMonthlyViewCommand { get; }
        public ICommand SetWeeklyViewCommand { get; }

        private DateTime _currentCalendarDate = DateTime.Now;

        // ===== Calendar Day Selection Properties =====
        private CalendarDayViewModel _selectedDay;
        public CalendarDayViewModel SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DayHabitStatusViewModel> _selectedDayHabits = new();
        public ObservableCollection<DayHabitStatusViewModel> SelectedDayHabits
        {
            get => _selectedDayHabits;
            set { _selectedDayHabits = value; OnPropertyChanged(); }
        }

        private int _selectedDayScore;
        public int SelectedDayScore
        {
            get => _selectedDayScore;
            set { _selectedDayScore = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedDayScoreText)); }
        }

        public string SelectedDayScoreText => SelectedDayScore.ToString();

        private string _selectedDayDateText;
        public string SelectedDayDateText
        {
            get => _selectedDayDateText;
            set { _selectedDayDateText = value; OnPropertyChanged(); }
        }

        private string _selectedDayLabel;
        public string SelectedDayLabel
        {
            get => _selectedDayLabel;
            set { _selectedDayLabel = value; OnPropertyChanged(); }
        }

        private string _selectedDayReflection;
        public string SelectedDayReflection
        {
            get => _selectedDayReflection;
            set { _selectedDayReflection = value; OnPropertyChanged(); }
        }

        // ===== Daily Note Auto-Save Properties =====
        private string _dailyNoteContent = "";
        private string _dailyNoteStatusText = "";
        private DispatcherTimer _noteSaveDebounceTimer;
        private string _currentNoteId;

        public string DailyNoteContent
        {
            get => _dailyNoteContent;
            set
            {
                if (_dailyNoteContent != value)
                {
                    _dailyNoteContent = value;
                    OnPropertyChanged();
                    DebounceSaveNote();
                }
            }
        }

        public string DailyNoteStatusText
        {
            get => _dailyNoteStatusText;
            set { _dailyNoteStatusText = value; OnPropertyChanged(); }
        }

        // ===== Statistics Modal Properties =====
        private bool _isStatisticsModalOpen = false;
        public bool IsStatisticsModalOpen
        {
            get => _isStatisticsModalOpen;
            set { _isStatisticsModalOpen = value; OnPropertyChanged(); }
        }

        // --- TOP-LEVEL STATISTICS PROPERTIES ---
        private int _growthQuotient;
        public int GrowthQuotient
        {
            get => _growthQuotient;
            set { _growthQuotient = value; OnPropertyChanged(); OnPropertyChanged(nameof(GrowthQuotientDashArray)); }
        }

        public string GrowthQuotientDashArray => $"{(_growthQuotient * 25.13 / 100.0).ToString("F2", CultureInfo.InvariantCulture)} 25.13";

        private bool _isPositiveGrowthTrend;
        public bool IsPositiveGrowthTrend
        {
            get => _isPositiveGrowthTrend;
            set { _isPositiveGrowthTrend = value; OnPropertyChanged(); }
        }

        private string _weeklyCompletionDateRange = "";
        public string WeeklyCompletionDateRange
        {
            get => _weeklyCompletionDateRange;
            set { _weeklyCompletionDateRange = value; OnPropertyChanged(); }
        }

        private int _weeklyCompletionOffset = 0;

        public bool CanNavigateNextWeek => _weeklyCompletionOffset < 0;

        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }

        private string _growthQuotientTrend;
        public string GrowthQuotientTrend
        {
            get => _growthQuotientTrend;
            set { _growthQuotientTrend = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WeeklyCompletionDay> _weeklyCompletionDays = new();
        public ObservableCollection<WeeklyCompletionDay> WeeklyCompletionDays
        {
            get => _weeklyCompletionDays;
            set { _weeklyCompletionDays = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TopHabitViewModel> _topHabits = new();
        public ObservableCollection<TopHabitViewModel> TopHabits
        {
            get => _topHabits;
            set { _topHabits = value; OnPropertyChanged(); }
        }

        private ObservableCollection<MilestoneViewModel> _milestones = new();
        public ObservableCollection<MilestoneViewModel> Milestones
        {
            get => _milestones;
            set { _milestones = value; OnPropertyChanged(); }
        }

        private HabitStatisticsViewModel _selectedHabitStats;
        public HabitStatisticsViewModel SelectedHabitStats
        {
            get => _selectedHabitStats;
            set { _selectedHabitStats = value; OnPropertyChanged(); }
        }

        private ObservableCollection<HabitPerformanceViewModel> _detailedHabitPerformances = new();
        public ObservableCollection<HabitPerformanceViewModel> DetailedHabitPerformances
        {
            get => _detailedHabitPerformances;
            set { _detailedHabitPerformances = value; OnPropertyChanged(); }
        }

        public ICommand OpenHabitStatisticsCommand { get; }
        public ICommand CloseHabitStatisticsCommand { get; }

        private DispatcherTimer _midnightTimer;

        public DashboardViewModel()
        {
            PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            SetMonthlyViewCommand = new RelayCommand(_ => IsMonthlyView = true);
            SetWeeklyViewCommand = new RelayCommand(_ => IsMonthlyView = false);
            SelectDayCommand = new RelayCommand(param => SelectDay(param as CalendarDayViewModel));
            OpenHabitStatisticsCommand = new RelayCommand(param => OpenHabitStatistics(param as HabitPerformanceViewModel));
            CloseHabitStatisticsCommand = new RelayCommand(_ => IsStatisticsModalOpen = false);
            PreviousWeekCommand = new RelayCommand(async _ => await NavigateWeek(-1));
            NextWeekCommand = new RelayCommand(async _ => await NavigateWeek(1));
            GenerateCalendarPlaceholder();
            
            // Initialize filter options
            UpdateFilterTranslations();
            
            // Initialize today's date and start timer to update it every minute
            UpdateTodayDate();
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) => UpdateTodayDate();
            timer.Start();

            SetupMidnightTimer();

            // Subscribe to theme changes to refresh calendar color dots
            HabitTracker.MainWindow.ThemeChanged += OnThemeChanged;
            
            // Subscribe to language changes to update filter lists
            Services.LocalizationService.Instance.PropertyChanged += OnLocalizationPropertyChanged;
        }

        private void SetupMidnightTimer()
        {
            _midnightTimer = new DispatcherTimer();
            _midnightTimer.Interval = GetTimeUntilMidnight();
            _midnightTimer.Tick += OnMidnight;
            _midnightTimer.Start();
        }

        private TimeSpan GetTimeUntilMidnight()
        {
            return DateTime.Today.AddDays(1) - DateTime.Now;
        }

        private async void OnMidnight(object sender, EventArgs e)
        {
            // Stop the timer and restart it for the next day
            _midnightTimer.Stop();
            _midnightTimer.Interval = GetTimeUntilMidnight();
            _midnightTimer.Start();

            await ResetHabits();
        }

        private async Task ResetHabits()
        {
            var allHabits = DailyHabits.Concat(WeeklyHabits).Concat(MonthlyHabits);
            foreach (var habit in allHabits)
            {
                habit.IsCompleted = false;
                habit.CurrentProgress = 0;
                // We do NOT reset PeriodProgress here. It will be recalculated
                // correctly upon next Initialization (or we could just leave it,
                // and it will naturally reset when the week/month boundaries are crossed during a reload).
            }

            // Reload habits to apply daily logic
            await LoadHabitsAsync();
        }

        private void OnLocalizationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Services.LocalizationService.CurrentLanguage))
            {
                UpdateFilterTranslations();
                if (Habits != null)
                {
                    foreach (var habit in Habits)
                    {
                        habit.NotifyDisplayPeriodChanged();
                    }
                }
            }
        }

        private void OnThemeChanged(bool isDark)
        {
            // Regenerate calendar so dot/badge colors adapt to the new theme
            _ = GenerateCalendarAsync();
        }

        private void UpdateFilterTranslations()
        {
            var loc = Services.LocalizationService.Instance;

            var selPrio = SelectedPriority?.Id;
            var selStat = SelectedStatus?.Id;
            var selFreq = SelectedFrequency?.Id;

            Priorities.Clear();
            Statuses.Clear();
            Frequencies.Clear();

            // Priorities
            Priorities.Add(new FilterItem { Id = "all", Name = loc.Get("Wszystkie", "All") });
            Priorities.Add(new FilterItem { Id = "1", Name = loc.PriorityHigh });
            Priorities.Add(new FilterItem { Id = "2", Name = loc.PriorityMedium });
            Priorities.Add(new FilterItem { Id = "3", Name = loc.PriorityLow });
            SelectedPriority = Priorities.FirstOrDefault(p => p.Id == selPrio) ?? Priorities.First();

            // Statuses
            Statuses.Add(new FilterItem { Id = "all", Name = loc.Get("Wszystkie", "All") });
            Statuses.Add(new FilterItem { Id = "active", Name = loc.StatusActive });
            Statuses.Add(new FilterItem { Id = "archived", Name = loc.StatusArchived });
            SelectedStatus = Statuses.FirstOrDefault(s => s.Id == selStat) ?? Statuses.First(s => s.Id == "active");

            // Frequencies
            Frequencies.Add(new FilterItem { Id = "all", Name = loc.Get("Wszystkie", "All") });
            Frequencies.Add(new FilterItem { Id = "daily", Name = loc.FreqDaily });
            Frequencies.Add(new FilterItem { Id = "weekly", Name = loc.FreqWeekly });
            Frequencies.Add(new FilterItem { Id = "monthly", Name = loc.FreqMonthly });
            Frequencies.Add(new FilterItem { Id = "specific", Name = loc.FreqSpecific });
            SelectedFrequency = Frequencies.FirstOrDefault(f => f.Id == selFreq) ?? Frequencies.First();
        }


        public void SwitchToMeasurements()
        {
            IsHabitsVisible = false;
            IsMeasurementsVisible = true;
            IsAddFormVisible = false;
            IsHomeVisible = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
        }

        public void SwitchToHabits()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = true;
            IsAddFormVisible = true;
            IsHomeVisible = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
        }

        public void SwitchToHome()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsHomeVisible = true;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
        }

        public void SwitchToSettings()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsHomeVisible = false;
            IsSettingsVisible = true;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
        }

        public void SwitchToCalendar()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsHomeVisible = false;
            IsSettingsVisible = false;
            IsCalendarVisible = true;
            IsStatisticsVisible = false;
        }

        public void SwitchToStatistics()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsHomeVisible = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = true;
            _weeklyCompletionOffset = 0;
            OnPropertyChanged(nameof(CanNavigateNextWeek));
        }

        private void SetStatus(string message, string color = "#FFFFFF")
        {
            // Implementacja metody SetStatus
            // Możesz dostosować StatusMessage i StatusColor do swoich potrzeb
            StatusMessage = message;
            StatusColor = color;
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private string _statusColor;
        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public async Task LoadHabitsAsync(){
            IsLoading = true;
            try{
                //pobranie nawykow z DB - tylko aktywne, tego uzytkownika
                var userId = SupabaseService.Client.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return;

                var response = await SupabaseService.Client.From<Habits>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Get();
                var habitsList = response.Models?.OrderBy(h => h.SortOrder).ToList() ?? new List<Habits>();

                if (HabitTypes.Count == 0)
                {
                    HabitTypes.Add(new HabitTypes { Id = "38e3ca04-c342-4520-9eb9-122c39339f1c", Type = "numeric", DisplayType = "Numeric", RequiresValue = true, DefaultUnit = "count" });
                    HabitTypes.Add(new HabitTypes { Id = "96a85519-7c6b-4787-ac1e-87137f1b2fb8", Type = "timer", DisplayType = "Timer", RequiresValue = true, DefaultUnit = "mins" });
                    HabitTypes.Add(new HabitTypes { Id = "dc75347f-83a0-42b6-a824-e3ac7428fae5", Type = "checkbox", DisplayType = "Checkbox", RequiresValue = false, DefaultUnit = null });
                }

                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);

                // Calculate week start (assuming Monday)
                int diff = (7 + (todayStart.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = todayStart.AddDays(-1 * diff);
                var weekEnd = weekStart.AddDays(7);

                // Calculate month start
                var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var earliestStart = weekStart < monthStart ? weekStart : monthStart;

                List<HabitLogs> allPeriodLogs = new List<HabitLogs>();
                try
                {
                    var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                        .Filter("log_date", Constants.Operator.GreaterThanOrEqual, earliestStart.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Filter("log_date", Constants.Operator.LessThan, todayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Order("created_date", Constants.Ordering.Descending)
                        .Get();
                    if (logsResponse.Models != null)
                    {
                        allPeriodLogs = logsResponse.Models;
                    }
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading habit logs: {logEx.Message}");
                }

                foreach (var habit in habitsList)
                {
                    var type = HabitTypes.FirstOrDefault(t => t.Id == habit.HabitTypeId);
                    if (type != null)
                    {
                        habit.DisplayTypeName = type.DisplayType;
                        habit.DefaultUnit = type.DefaultUnit;
                    }

                    var habitLogs = allPeriodLogs.Where(l => l.HabitId == habit.Id).ToList();
                    var todayLog = habitLogs.FirstOrDefault(l => l.LogDate >= todayStart && l.LogDate < todayEnd);

                    if (todayLog != null)
                    {
                        habit.IsCompleted = todayLog.IsCompleted;
                        habit.CurrentProgress = todayLog.NumericValue;
                    }
                    else
                    {
                        habit.IsCompleted = false;
                        habit.CurrentProgress = 0;
                    }

                    // Calculate Period Progress
                    if (string.Equals(habit.Period, "weekly", StringComparison.OrdinalIgnoreCase))
                    {
                        var weeklyLogs = habitLogs.Where(l => l.LogDate >= weekStart && l.LogDate < weekEnd).ToList();
                        if (habit.IsCheckboxType)
                            habit.PeriodProgress = weeklyLogs.Count(l => l.IsCompleted);
                        else
                            habit.PeriodProgress = weeklyLogs.Sum(l => l.NumericValue);
                    }
                    else if (string.Equals(habit.Period, "monthly", StringComparison.OrdinalIgnoreCase))
                    {
                        var monthlyLogs = habitLogs.Where(l => l.LogDate >= monthStart && l.LogDate < monthEnd).ToList();
                        if (habit.IsCheckboxType)
                            habit.PeriodProgress = monthlyLogs.Count(l => l.IsCompleted);
                        else
                            habit.PeriodProgress = monthlyLogs.Sum(l => l.NumericValue);
                    }
                    else
                    {
                        habit.PeriodProgress = habit.CurrentProgress;
                    }

                    // Wire up PropertyChanged to save progress to DB
                    habit.PropertyChanged -= Habit_PropertyChanged;
                    habit.PropertyChanged += Habit_PropertyChanged;
                }

                Habits = new ObservableCollection<Habits>(habitsList);

                // Populate segregated collections for the Home view
                DailyHabits.Clear();
                WeeklyHabits.Clear();
                MonthlyHabits.Clear();

                foreach (var h in habitsList)
                {
                    if (h.IsArchived) continue;

                    if (string.Equals(h.Period, "weekly", StringComparison.OrdinalIgnoreCase))
                    {
                        WeeklyHabits.Add(h);
                    }
                    else if (string.Equals(h.Period, "monthly", StringComparison.OrdinalIgnoreCase))
                    {
                        MonthlyHabits.Add(h);
                    }
                    else
                    {
                        // Daily or Flexible
                        // For non-flexible daily, it must be scheduled for today
                        if (h.IsFlexible || DailyScoreCalculator.IsScheduledForDay(h.DaysOfWeek, DateTime.Today.DayOfWeek))
                        {
                            DailyHabits.Add(h);
                        }
                    }
                }
                OnPropertyChanged(nameof(DailyHabits));
                OnPropertyChanged(nameof(WeeklyHabits));
                OnPropertyChanged(nameof(MonthlyHabits));
                
                // Default view used by Habit Manager (if any) or old UI
                var defaultView = CollectionViewSource.GetDefaultView(Habits);
                defaultView.Filter = (obj) => 
                {
                    if (obj is Habits h)
                    {
                        if (h.IsArchived) return false;
                        if (h.IsFlexible) return true;
                        if (!DailyScoreCalculator.IsScheduledForDay(h.DaysOfWeek, DateTime.Today.DayOfWeek)) return false;
                        return true;
                    }
                    return true;
                };

                // Independent view for Habit Manager
                FilteredHabits = new CollectionViewSource { Source = Habits }.View;
                FilteredHabits.Filter = FilterHabit;

                // Calculate daily progress for the circular indicator
                RecalculateDailyProgress();
            }
            catch(Exception ex){
                SetStatus($"Failed to download habits: {ex.Message}","FFD32F2F");
            }
            finally{
                IsLoading=false;
            }
        }

        private bool FilterHabit(object obj)
        {
            if (obj is Habits habit)
            {
                // Status Filter
                if (SelectedStatus != null && SelectedStatus.Id != "all")
                {
                    if (SelectedStatus.Id == "active" && habit.IsArchived) return false;
                    if (SelectedStatus.Id == "archived" && !habit.IsArchived) return false;
                }

                // Priority Filter
                if (SelectedPriority != null && SelectedPriority.Id != "all")
                {
                    if (habit.Priority.ToString() != SelectedPriority.Id) return false;
                }

                // Frequency Filter
                if (SelectedFrequency != null && SelectedFrequency.Id != "all")
                {
                    if (!string.Equals(habit.Period, SelectedFrequency.Id, StringComparison.OrdinalIgnoreCase)) return false;
                }

                return true;
            }
            return false;
        }

        private async void Habit_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Habits habit && (e.PropertyName == nameof(habit.IsCompleted) || e.PropertyName == nameof(habit.CurrentProgress)))
            {
                await SaveHabitLogAsync(habit);
                
                // Optimistically update PeriodProgress without reloading from DB
                if (string.Equals(habit.Period, "weekly", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(habit.Period, "monthly", StringComparison.OrdinalIgnoreCase))
                {
                    // To do this perfectly we should reload logs.
                    // But an optimistic update works for simple cases. 
                    // However, we just trigger a full refresh in background to be safe.
                    _ = Task.Run(async () => {
                        await App.Current.Dispatcher.InvokeAsync(async () => {
                            await RecalculatePeriodProgress(habit);
                        });
                    });
                }
                else
                {
                    habit.PeriodProgress = habit.CurrentProgress;
                }

                RecalculateDailyProgress();
                _ = GenerateCalendarAsync(); // Keep calendar in sync with home
            }
        }

        private async Task RecalculatePeriodProgress(Habits habit)
        {
            // Simple logic: fetch this week's / month's logs for this specific habit to update its period progress
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);
            int diff = (7 + (todayStart.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = todayStart.AddDays(-1 * diff);
            var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
            
            var start = string.Equals(habit.Period, "monthly", StringComparison.OrdinalIgnoreCase) ? monthStart : weekStart;

            try
            {
                var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                    .Filter("habit_id", Constants.Operator.Equals, habit.Id)
                    .Filter("log_date", Constants.Operator.GreaterThanOrEqual, start.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Filter("log_date", Constants.Operator.LessThan, todayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Get();
                
                if (logsResponse.Models != null)
                {
                    if (habit.IsCheckboxType)
                        habit.PeriodProgress = logsResponse.Models.Count(l => l.IsCompleted);
                    else
                        habit.PeriodProgress = logsResponse.Models.Sum(l => l.NumericValue);
                }
            }
            catch { /* Ignore errors during optimistic update */ }
        }

        public async Task SaveHabitLogAsync(Habits habit)
        {
            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);
                
                var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                    .Filter("habit_id", Constants.Operator.Equals, habit.Id)
                    .Filter("log_date", Constants.Operator.GreaterThanOrEqual, todayStart.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Filter("log_date", Constants.Operator.LessThan, todayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Order("created_date", Constants.Ordering.Descending)
                    .Get();
                
                var existingLog = logsResponse.Models?.FirstOrDefault();
                
                if (existingLog != null)
                {
                    existingLog.IsCompleted = habit.IsCompleted;
                    existingLog.NumericValue = habit.CurrentProgress;
                    existingLog.UpdatedTime = DateTime.UtcNow;
                    
                    await SupabaseService.Client.From<HabitLogs>()
                        .Filter("id", Constants.Operator.Equals, existingLog.Id)
                        .Update(existingLog);
                }
                else
                {
                    var newLog = new HabitLogs
                    {
                        HabitId = habit.Id,
                        UserId = userId,
                        LogDate = todayStart,
                        IsCompleted = habit.IsCompleted,
                        NumericValue = habit.CurrentProgress,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedTime = DateTime.UtcNow,
                        Status = 1
                    };
                    
                    await SupabaseService.Client.From<HabitLogs>().Insert(newLog);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving habit log: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"BŁĄD ZAPISU DO BAZY:\n{ex.Message}");
            }
        }

        //Metoda ladujaca dane
        public async Task LoadFormDataAsync(){
            // Populate defaults first to ensure we have working types even if DB query returns empty (due to RLS or other issues)
            HabitTypes.Clear();
            HabitTypes.Add(new HabitTypes { Id = "38e3ca04-c342-4520-9eb9-122c39339f1c", Type = "numeric", DisplayType = "Numeric", RequiresValue = true, DefaultUnit = "count" });
            HabitTypes.Add(new HabitTypes { Id = "96a85519-7c6b-4787-ac1e-87137f1b2fb8", Type = "timer", DisplayType = "Timer", RequiresValue = true, DefaultUnit = "mins" });
            HabitTypes.Add(new HabitTypes { Id = "dc75347f-83a0-42b6-a824-e3ac7428fae5", Type = "checkbox", DisplayType = "Checkbox", RequiresValue = false, DefaultUnit = null });
            SelectedType = HabitTypes.FirstOrDefault();

            try{
                var types = await SupabaseService.Client.From<HabitTypes>().Get();
                if (types.Models != null && types.Models.Count > 0)
                {
                    HabitTypes.Clear();
                    foreach(var type in types.Models)
                        HabitTypes.Add(type);
                    SelectedType = HabitTypes.FirstOrDefault();
                }
            }
            catch(Exception ex){
                System.Diagnostics.Debug.WriteLine($"Error loading habit types from DB: {ex.Message}");
            }

            try{
                // Load colors
                var colors = await SupabaseService.Client.From<Colors>().Get();
                Colors.Clear();
                if (colors.Models != null && colors.Models.Count > 0)
                {
                    foreach(var color in colors.Models)
                        Colors.Add(color);
                }
            }
            catch(Exception ex){
                System.Diagnostics.Debug.WriteLine($"Error loading colors from DB: {ex.Message}");
            }
        }

        // Add a habit chosen from the built-in list
        public async Task AddBuiltInHabitAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedBuiltInHabit))
            {
                return;
            }

            // Set name and delegate to CreateHabitAsync to reuse validation/creation logic
            NewHabitName = SelectedBuiltInHabit;

            // Ensure there is a type selected
            if (SelectedType == null) SelectedType = HabitTypes.FirstOrDefault();

            await CreateHabitAsync();
        }

        //Dodawanie nawyku
        public async Task CreateHabitAsync(){
            if(string.IsNullOrWhiteSpace(NewHabitName)){
                return;
            }

            try{
                IsLoading = true;

                // Look up HabitType UUID by display name
                var habitType = HabitTypes.FirstOrDefault(t => 
                    string.Equals(t.DisplayType, NewHabitType, StringComparison.OrdinalIgnoreCase));
                
                if (habitType == null)
                {
                    return;
                }

                if (EditingHabit != null)
                {
                    string iconToSave = NewHabitIcon == "❓" ? "" : NewHabitIcon;
                    EditingHabit.Name = NewHabitName;
                    EditingHabit.HabitTypeId = habitType.Id;
                    EditingHabit.Icon = iconToSave;
                    EditingHabit.Period = NewHabitFrequency;
                    EditingHabit.TargetFrequency = (int)NewHabitGoal;
                    EditingHabit.DaysOfWeek = NewHabitDaysOfWeek;
                    EditingHabit.Priority = NewHabitPriority;
                    EditingHabit.Unit = NewHabitUnit;

                    await SupabaseService.Client.From<Habits>()
                        .Filter("id", Constants.Operator.Equals, EditingHabit.Id)
                        .Update(EditingHabit);

                    // Updated silently
                }
                else
                {
                    var habit = new Habits{
                        Name = NewHabitName,
                        HabitTypeId = habitType.Id,
                        UserId = SupabaseService.Client.Auth.CurrentUser.Id,
                        Icon = NewHabitIcon == "❓" ? "" : NewHabitIcon,
                        Period = NewHabitFrequency,
                        TargetFrequency = (int)NewHabitGoal,
                        DaysOfWeek = NewHabitDaysOfWeek,
                        Priority = NewHabitPriority,
                        Unit = NewHabitUnit,
                        IsFlexible = false,
                        IsArchived = false,
                        IsSystem = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await SupabaseService.Client.From<Habits>().Insert(habit);

                    // Created silently
                }

                //Reset formularza i refresh listy
                ResetAddHabitForm();
                IsAddHabitModalOpen = false;
                await LoadHabitsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving habit: {ex.Message}");
            }
            finally{IsLoading = false;}
        }



        // Usuwanie (archiwizacja) nawyku
        public async Task DeleteHabitAsync(Habits habit){
            try{
                habit.IsArchived = true;
                await SupabaseService.Client.From<Habits>()
                    .Filter("id", Constants.Operator.Equals, habit.Id)
                    .Set(h => h.IsArchived, true)
                    .Update();
                await LoadHabitsAsync();
            }
            catch(Exception ex){
                System.Diagnostics.Debug.WriteLine($"Error archiving habit: {ex.Message}");
            }
        }

        // Reset formularza Add Habit
        public void ResetAddHabitForm(){
            EditingHabit = null;
            NewHabitName = string.Empty;
            NewHabitType = "Numeric";
            NewHabitPriority = 2;
            NewHabitFrequency = "Daily";
            NewHabitIcon = "❓";
            NewHabitDaysOfWeek = 127;
            NewHabitGoal = 1;
            NewHabitUnit = "count";
        }
        
        private string _todayDate;
        public string TodayDate
        {
            get => _todayDate;
            set { _todayDate = value; OnPropertyChanged(); }
        }

        private void UpdateTodayDate()
        {
            var culture = CultureInfo.GetCultureInfo(Services.LocalizationService.Instance.CurrentLanguage == "en" ? "en-US" : "pl-PL");
            TodayDate = DateTime.Now.ToString("yyyy-MM-dd, dddd", culture);
        }

        // ===== Daily Progress Circle Properties =====
        private int _dailyProgressPercentage;
        public int DailyProgressPercentage
        {
            get => _dailyProgressPercentage;
            set 
            { 
                _dailyProgressPercentage = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(DailyProgressText)); 
                OnPropertyChanged(nameof(DailyProgressDashArray)); 
                OnPropertyChanged(nameof(DailyProgressStrokeThickness)); 
            }
        }

        public string DailyProgressText => $"{DailyProgressPercentage}%";

        public double DailyProgressStrokeThickness => DailyProgressPercentage > 0 ? 12 : 0;

        /// <summary>
        /// StrokeDashArray for the progress arc. Circle circumference = 2π×48 ≈ 301.59.
        /// WPF StrokeDashArray units are in multiples of StrokeThickness (12),
        /// so the total dash length is 301.59 / 12 ≈ 25.13.
        /// </summary>
        public System.Windows.Media.DoubleCollection DailyProgressDashArray
        {
            get
            {
                double totalUnits = (2 * Math.PI * 48) / 12; // ≈ 25.13
                double filled = totalUnits * DailyProgressPercentage / 100.0;
                double gap = totalUnits - filled;
                if (filled <= 0) filled = 0;
                return new System.Windows.Media.DoubleCollection { filled, gap > 0 ? gap : 0 };
            }
        }

        private int _progressAnimationId = 0;
        private int _targetDailyProgressPercentage = 0;

        private async void AnimateProgressTo(int targetProgress)
        {
            _targetDailyProgressPercentage = targetProgress;
            int currentId = ++_progressAnimationId;

            while (_dailyProgressPercentage != _targetDailyProgressPercentage)
            {
                if (_progressAnimationId != currentId)
                    return; // Another animation started

                if (_dailyProgressPercentage < _targetDailyProgressPercentage)
                {
                    int step = (_targetDailyProgressPercentage - _dailyProgressPercentage) > 20 ? 2 : 1;
                    DailyProgressPercentage += step;
                    if (DailyProgressPercentage > _targetDailyProgressPercentage)
                        DailyProgressPercentage = _targetDailyProgressPercentage;
                }
                else
                {
                    int step = (_dailyProgressPercentage - _targetDailyProgressPercentage) > 20 ? 2 : 1;
                    DailyProgressPercentage -= step;
                    if (DailyProgressPercentage < _targetDailyProgressPercentage)
                        DailyProgressPercentage = _targetDailyProgressPercentage;
                }

                await Task.Delay(15);
            }
        }

        /// <summary>
        /// Recalculates daily progress using the centralised DailyScoreCalculator.
        /// Builds synthetic HabitLogs from in-memory state so the result updates
        /// instantly without waiting for the database round-trip.
        /// </summary>
        private void RecalculateDailyProgress()
        {
            if (Habits == null || Habits.Count == 0)
            {
                AnimateProgressTo(0);
                return;
            }

            var activeHabits = Habits.Where(h => !h.IsArchived).ToList();
            if (activeHabits.Count == 0)
            {
                AnimateProgressTo(0);
                return;
            }

            var habitTypeMap = HabitTypes?.ToDictionary(t => t.Id, t => t)
                               ?? new Dictionary<string, HabitTypes>();

            var result = DailyScoreCalculator.CalculateFromLiveState(activeHabits, habitTypeMap);
            AnimateProgressTo(result.Percentage);
        }

        private void ChangeMonth(int monthsToAdd)
        {
            _currentCalendarDate = _currentCalendarDate.AddMonths(monthsToAdd);
            _ = GenerateCalendarAsync();
        }

        /// <summary>
        /// Placeholder calendar used at construction before user is logged in.
        /// </summary>
        private void GenerateCalendarPlaceholder()
        {
            var culture = CultureInfo.GetCultureInfo(Services.LocalizationService.Instance.CurrentLanguage == "en" ? "en-US" : "pl-PL");
            CurrentMonthYear = _currentCalendarDate.ToString("MMMM yyyy", culture);
            var days = new ObservableCollection<CalendarDayViewModel>();
            var firstDayOfMonth = new DateTime(_currentCalendarDate.Year, _currentCalendarDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentCalendarDate.Year, _currentCalendarDate.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (startDayOfWeek == 0) startDayOfWeek = 7;
            var startDate = firstDayOfMonth.AddDays(-(startDayOfWeek - 1));
            int totalDays = 42;
            if (startDayOfWeek == 1 && daysInMonth == 28) totalDays = 28;
            else if (startDayOfWeek + daysInMonth - 1 <= 35) totalDays = 35;

            for (int i = 0; i < totalDays; i++)
            {
                var date = startDate.AddDays(i);
                bool isCurrentMonth = date.Month == _currentCalendarDate.Month;
                bool isToday = date.Date == DateTime.Today;
                days.Add(new CalendarDayViewModel
                {
                    Date = date,
                    DayNumber = date.ToString("dd"),
                    PercentageText = "",
                    IsCurrentMonth = isCurrentMonth,
                    IsToday = isToday,
                    IsSelected = isToday,
                    BadgeColor = "#F0F2F5",
                    DotColor = "Transparent"
                });
            }
            CalendarDays = days;
        }

        /// <summary>
        /// Generates the calendar grid with real data from Supabase.
        /// Calculates day-by-day completion percentages based on habit_logs.
        /// </summary>
        public async Task GenerateCalendarAsync()
        {
            var culture = CultureInfo.GetCultureInfo(Services.LocalizationService.Instance.CurrentLanguage == "en" ? "en-US" : "pl-PL");
            CurrentMonthYear = _currentCalendarDate.ToString("MMMM yyyy", culture);
            var days = new ObservableCollection<CalendarDayViewModel>();

            var firstDayOfMonth = new DateTime(_currentCalendarDate.Year, _currentCalendarDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentCalendarDate.Year, _currentCalendarDate.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (startDayOfWeek == 0) startDayOfWeek = 7;
            var startDate = firstDayOfMonth.AddDays(-(startDayOfWeek - 1));
            int totalDays = 42;
            if (startDayOfWeek == 1 && daysInMonth == 28) totalDays = 28;
            else if (startDayOfWeek + daysInMonth - 1 <= 35) totalDays = 35;

            var endDate = startDate.AddDays(totalDays - 1);

            // Fetch data from Supabase
            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            List<Habits> userHabits = new();
            List<HabitLogs> monthLogs = new();

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var habitsResponse = await SupabaseService.Client.From<Habits>()
                        .Filter("user_id", Constants.Operator.Equals, userId)
                        .Filter("is_archived", Constants.Operator.Equals, "false")
                        .Get();
                    userHabits = habitsResponse.Models ?? new List<Habits>();

                    var utcStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
                    var utcEndDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1); // include the whole end day

                    // Use DateTime objects with UTC kind for filtering
                    var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                        .Filter("log_date", Constants.Operator.GreaterThanOrEqual, utcStartDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Filter("log_date", Constants.Operator.LessThan, utcEndDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Get();
                        
                    var habitIds = new HashSet<string>(userHabits.Select(h => h.Id));
                    monthLogs = (logsResponse.Models ?? new List<HabitLogs>())
                                .Where(l => habitIds.Contains(l.HabitId)).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading calendar data: {ex.Message}");
                }
            }

            var logsByDate = monthLogs.GroupBy(l => l.LogDate.Date)
                                     .ToDictionary(g => g.Key, g => g.ToList());

            // Build habit type map
            Dictionary<string, HabitTypes> habitTypeMap = new();
            try
            {
                if (HabitTypes != null && HabitTypes.Any())
                    habitTypeMap = HabitTypes.ToDictionary(t => t.Id, t => t);
                else
                {
                    var typesResp = await SupabaseService.Client.From<HabitTypes>().Get();
                    habitTypeMap = typesResp.Models.ToDictionary(t => t.Id, t => t);
                }
            }
            catch { }

            for (int i = 0; i < totalDays; i++)
            {
                var date = startDate.AddDays(i);
                bool isCurrentMonth = date.Month == _currentCalendarDate.Month;
                bool isToday = date.Date == DateTime.Today;
                bool isFutureDay = date.Date > DateTime.Today;

                int percentage = -1;
                string badgeColor, dotColor;

                if (isFutureDay)
                {
                    // Future days: no score calculation, no percentage, no colored dot
                    GetDayColors(-1, false, out badgeColor, out dotColor);
                }
                else
                {
                    DailyScoreCalculator.DailyScoreResult scoreResult;

                    if (isToday && Habits != null && Habits.Count > 0)
                    {
                        // TODAY: use in-memory state to guarantee the calendar percentage
                        // matches the home dashboard exactly (avoids DB race conditions).
                        var activeHabits = Habits.Where(h => !h.IsArchived).ToList();
                        scoreResult = DailyScoreCalculator.CalculateFromLiveState(activeHabits, habitTypeMap);
                    }
                    else
                    {
                        // Historical days: use database logs
                        var dayLogsList = logsByDate.TryGetValue(date.Date, out var dayLogs) ? dayLogs : new List<HabitLogs>();
                        scoreResult = DailyScoreCalculator.CalculateDailyScore(date, userHabits, dayLogsList, habitTypeMap);
                    }

                    percentage = scoreResult.PlannedCount > 0 ? scoreResult.Percentage : -1;
                    GetDayColors(percentage, isToday, out badgeColor, out dotColor);
                }

                days.Add(new CalendarDayViewModel
                {
                    Date = date,
                    DayNumber = date.ToString("dd"),
                    PercentageText = isCurrentMonth && !isFutureDay && percentage >= 0 ? $"{percentage}%" : "",
                    IsCurrentMonth = isCurrentMonth,
                    IsToday = isToday,
                    IsSelected = isToday,
                    BadgeColor = badgeColor,
                    DotColor = dotColor
                });
            }

            CalendarDays = days;

            // Auto-select today
            var todayDay = days.FirstOrDefault(d => d.IsToday);
            if (todayDay != null)
                SelectDay(todayDay);
        }

        // NOTE: DailyScoreCalculator.IsScheduledForDay, GetPriorityWeight, and CalculateHabitContribution
        // have been moved to Services/DailyScoreCalculator.cs as the single source of truth.

        private void GetDayColors(int percentage, bool isToday, out string badgeColor, out string dotColor)
        {
            // Detect current theme by checking a known dark-mode brush value
            bool isDark = false;
            try
            {
                var appBgBrush = System.Windows.Application.Current.Resources["AppBgBrush"] as System.Windows.Media.SolidColorBrush;
                if (appBgBrush != null)
                {
                    // Dark mode AppBgBrush is dark (R+G+B < 300)
                    var c = appBgBrush.Color;
                    isDark = (c.R + c.G + c.B) < 300;
                }
            }
            catch { }

            if (percentage < 0)
            {
                badgeColor = isDark ? "#2A2A2A" : "#E5E7EB";
                dotColor = isDark ? "#A0A0A0" : "#6B7280"; // Gray text for future days
                return;
            }

            dotColor = (percentage >= 25 && percentage <= 70) ? "#374151" : "#FFFFFF"; // Dark text on yellow/orange, white on red/green

            // Snap percentage to nearest 5%
            percentage = (percentage / 5) * 5;

            // Keyframes for smooth gradient (SLIGHTLY MISTY VIBRANT)
            // Red -> Orange -> Yellow -> Light Green -> Dark Green
            (int p, byte r, byte g, byte b)[] lightFrames = {
                (0,   226,  92,  92), // Misty Red
                (25,  235, 132,  70), // Misty Orange
                (50,  224, 186,  67), // Misty Yellow
                (75,   82, 191, 122), // Misty Light Green
                (100,  47, 143,  81)  // Misty Dark Green
            };

            (int p, byte r, byte g, byte b)[] darkFrames = {
                (0,   189,  64,  64), // Dark Misty Red
                (25,  196, 103,  49), // Dark Misty Orange
                (50,  186, 148,  45), // Dark Misty Yellow
                (75,   46, 148,  83), // Dark Misty Light Green
                (100,  34, 107,  59)  // Dark Misty Dark Green
            };

            var frames = isDark ? darkFrames : lightFrames;

            for (int i = 0; i < frames.Length - 1; i++)
            {
                if (percentage >= frames[i].p && percentage <= frames[i+1].p)
                {
                    float t = (float)(percentage - frames[i].p) / (frames[i+1].p - frames[i].p);
                    byte r = (byte)(frames[i].r + t * (frames[i+1].r - frames[i].r));
                    byte g = (byte)(frames[i].g + t * (frames[i+1].g - frames[i].g));
                    byte b = (byte)(frames[i].b + t * (frames[i+1].b - frames[i].b));
                    badgeColor = $"#{r:X2}{g:X2}{b:X2}";
                    return;
                }
            }

            badgeColor = $"#{frames[^1].r:X2}{frames[^1].g:X2}{frames[^1].b:X2}";
        }

        /// <summary>
        /// Selects a day in the calendar and loads its habits + reflection from DB.
        /// </summary>
        private async void SelectDay(CalendarDayViewModel day)
        {
            if (day == null) return;

            foreach (var d in CalendarDays)
                d.IsSelected = false;
            day.IsSelected = true;
            SelectedDay = day;

            var culture = CultureInfo.GetCultureInfo(Services.LocalizationService.Instance.CurrentLanguage == "en" ? "en-US" : "pl-PL");
            SelectedDayDateText = day.Date.ToString("dddd, MMMM d", culture);

            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                Dictionary<string, HabitTypes> habitTypeMap = new();
                try
                {
                    if (HabitTypes != null && HabitTypes.Any())
                        habitTypeMap = HabitTypes.ToDictionary(t => t.Id, t => t);
                    else
                    {
                        var typesResp = await SupabaseService.Client.From<HabitTypes>().Get();
                        habitTypeMap = (typesResp.Models ?? new List<HabitTypes>()).ToDictionary(t => t.Id, t => t);
                    }
                }
                catch { }

                DailyScoreCalculator.DailyScoreResult scoreResult;
                bool isToday = day.Date.Date == DateTime.Today;

                if (isToday && Habits != null && Habits.Count > 0)
                {
                    // TODAY: use in-memory state to guarantee the detail panel
                    // matches the home dashboard exactly (avoids DB race conditions).
                    var activeHabits = Habits.Where(h => !h.IsArchived).ToList();
                    scoreResult = DailyScoreCalculator.CalculateFromLiveState(activeHabits, habitTypeMap);
                }
                else
                {
                    // Historical day: fetch from database
                    var habitsResponse = await SupabaseService.Client.From<Habits>()
                        .Filter("user_id", Constants.Operator.Equals, userId)
                        .Filter("is_archived", Constants.Operator.Equals, "false")
                        .Get();
                    var userHabits = habitsResponse.Models ?? new List<Habits>();

                    var utcDayStart = DateTime.SpecifyKind(day.Date.Date, DateTimeKind.Utc);
                    var utcDayEnd = utcDayStart.AddDays(1);
                    var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                        .Filter("log_date", Constants.Operator.GreaterThanOrEqual, utcDayStart.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Filter("log_date", Constants.Operator.LessThan, utcDayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                        .Order("created_date", Constants.Ordering.Descending)
                        .Get();
                    var habitIds = new HashSet<string>(userHabits.Select(h => h.Id));
                    var rawLogs = logsResponse.Models ?? new List<HabitLogs>();
                    var dayLogs = rawLogs.Where(l => habitIds.Contains(l.HabitId)).ToList();

                    scoreResult = DailyScoreCalculator.CalculateDailyScore(day.Date, userHabits, dayLogs, habitTypeMap);
                }

                var habitStatuses = new ObservableCollection<DayHabitStatusViewModel>();
                foreach (var detail in scoreResult.Details)
                {
                    habitStatuses.Add(new DayHabitStatusViewModel
                    {
                        HabitId = detail.HabitId,
                        HabitName = detail.HabitName,
                        IsCompleted = detail.IsCompleted,
                        StatusText = detail.StatusText,
                        HabitType = detail.TypeName
                    });
                }

                SelectedDayHabits = habitStatuses;
                SelectedDayScore = scoreResult.Percentage;

                if (scoreResult.PlannedCount == 0) SelectedDayLabel = "No habits planned";
                else if (SelectedDayScore == 100) SelectedDayLabel = "Perfect Growth Day";
                else if (SelectedDayScore >= 75) SelectedDayLabel = "Great Progress";
                else if (SelectedDayScore >= 50) SelectedDayLabel = "Decent Effort";
                else SelectedDayLabel = "Room for Growth";

                // Fetch reflection/note
                try
                {
                    var noteDateStr = day.Date.ToString("yyyy-MM-dd");
                    var notesResponse = await SupabaseService.Client.From<Notes>()
                        .Filter("user_id", Constants.Operator.Equals, userId)
                        .Filter("note_date", Constants.Operator.Equals, noteDateStr)
                        .Get();
                    var note = (notesResponse.Models ?? new List<Notes>()).FirstOrDefault();
                    SelectedDayReflection = note?.Content ?? "";
                }
                catch { SelectedDayReflection = ""; }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading day details: {ex.Message}");
                SelectedDayLabel = "Error: " + ex.Message;
                SelectedDayHabits = new ObservableCollection<DayHabitStatusViewModel>();
            }
        }

        /// <summary>
        /// Opens the statistics modal for a specific habit.
        /// </summary>
        private async void OpenHabitStatistics(HabitPerformanceViewModel perf)
        {
            if (perf == null) return;
            var stats = new HabitStatisticsViewModel();
            await stats.CalculateStatisticsAsync(perf.Habit, HabitTypes.ToList());
            SelectedHabitStats = stats;
            IsStatisticsModalOpen = true;
        }

        private async Task NavigateWeek(int direction)
        {
            if (direction > 0 && _weeklyCompletionOffset >= 0) return;
            _weeklyCompletionOffset += direction;
            OnPropertyChanged(nameof(CanNavigateNextWeek));
            await LoadStatisticsDataAsync();
        }

        /// <summary>
        /// Loads detailed habit performances for the Statistics tab.
        /// </summary>
        public async Task LoadStatisticsDataAsync()
        {
            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var habitsResponse = await SupabaseService.Client.From<Habits>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Filter("is_archived", Constants.Operator.Equals, "false")
                    .Get();
                var userHabits = habitsResponse.Models;

                Dictionary<string, HabitTypes> habitTypeMap = new();
                try
                {
                    if (HabitTypes != null && HabitTypes.Any())
                        habitTypeMap = HabitTypes.ToDictionary(t => t.Id, t => t);
                    else
                    {
                        var typesResp = await SupabaseService.Client.From<HabitTypes>().Get();
                        habitTypeMap = typesResp.Models.ToDictionary(t => t.Id, t => t);
                    }
                }
                catch { }

                // Fetch last 90 days of logs (with UTC boundaries)
                var ninetyDaysAgo = DateTime.SpecifyKind(DateTime.Today.AddDays(-90), DateTimeKind.Utc);
                var todayEndUtc = DateTime.SpecifyKind(DateTime.Today.AddDays(14), DateTimeKind.Utc);
                var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                    .Filter("log_date", Constants.Operator.GreaterThanOrEqual, ninetyDaysAgo.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Filter("log_date", Constants.Operator.LessThan, todayEndUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
                    .Get();
                var habitIds = new HashSet<string>(userHabits.Select(h => h.Id));
                var allLogs = (logsResponse.Models ?? new List<HabitLogs>())
                              .Where(l => habitIds.Contains(l.HabitId)).ToList();
                var logsByHabit = allLogs.GroupBy(l => l.HabitId).ToDictionary(g => g.Key, g => g.ToList());

                var performances = new ObservableCollection<HabitPerformanceViewModel>();

                foreach (var habit in userHabits)
                {
                    habitTypeMap.TryGetValue(habit.HabitTypeId ?? "", out var habitType);
                    if (habitType != null)
                    {
                        habit.DisplayTypeName = habitType.DisplayType;
                        habit.DefaultUnit = habitType.DefaultUnit;
                    }
                    var typeName = habitType?.Type?.ToLower() ?? "checkbox";
                    logsByHabit.TryGetValue(habit.Id, out var habitLogs);
                    habitLogs ??= new List<HabitLogs>();

                    // Calculate consistency (last 30 days)
                    int scheduledDays = 0, completedDays = 0;
                    for (int d = 0; d < 30; d++)
                    {
                        var date = DateTime.Today.AddDays(-d);
                        if (habit.CreatedDate.Date > date.Date) continue;
                        if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;
                        scheduledDays++;

                        var log = habitLogs.FirstOrDefault(l => l.LogDate.Date == date.Date);
                        if (log != null)
                        {
                            bool done = typeName == "checkbox" ? log.IsCompleted : log.NumericValue >= habit.TargetFrequency;
                            if (done) completedDays++;
                        }
                    }
                    int consistency = scheduledDays > 0 ? (int)Math.Round((double)completedDays / scheduledDays * 100) : 0;

                    // Calculate current streak (Option A: skip non-scheduled days)
                    int currentStreak = 0;
                    for (int d = 0; d < 365; d++)
                    {
                        var date = DateTime.Today.AddDays(-d);
                        if (habit.CreatedDate.Date > date.Date) break;
                        if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;

                        var log = habitLogs.FirstOrDefault(l => l.LogDate.Date == date.Date);
                        bool done = log != null && (typeName == "checkbox" ? log.IsCompleted : log.NumericValue >= habit.TargetFrequency);

                        if (done) currentStreak++;
                        else break;
                    }

                    // Calculate trend (last 7 vs previous 7 days)
                    int recent7 = 0, prev7 = 0, recent7Total = 0, prev7Total = 0;
                    for (int d = 0; d < 14; d++)
                    {
                        var date = DateTime.Today.AddDays(-d);
                        if (habit.CreatedDate.Date > date.Date) continue;
                        if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;

                        var log = habitLogs.FirstOrDefault(l => l.LogDate.Date == date.Date);
                        bool done = log != null && (typeName == "checkbox" ? log.IsCompleted : log.NumericValue >= habit.TargetFrequency);

                        if (d < 7) { recent7Total++; if (done) recent7++; }
                        else { prev7Total++; if (done) prev7++; }
                    }

                    double recentRate = recent7Total > 0 ? (double)recent7 / recent7Total * 100 : 0;
                    double prevRate = prev7Total > 0 ? (double)prev7 / prev7Total * 100 : 0;
                    double trendChange = recentRate - prevRate;
                    string trendText = trendChange >= 0 ? $"+{(int)trendChange}%" : $"{(int)trendChange}%";

                    performances.Add(new HabitPerformanceViewModel
                    {
                        Habit = habit,
                        HabitName = habit.Name,
                        HabitDescription = $"{habitType?.DisplayType ?? typeName}",
                        Consistency = consistency,
                        TrendText = trendText,
                        IsPositiveTrend = trendChange >= 0,
                        CurrentStreak = currentStreak,
                        HabitTypeName = typeName,
                        HabitIcon = habit.Icon ?? "🌱"
                    });
                }

                DetailedHabitPerformances = performances;

                // --- CALCULATE TOP-LEVEL STATISTICS ---
                CalculateTopStatistics(userHabits, habitTypeMap, allLogs);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        private void CalculateTopStatistics(List<Habits> userHabits, Dictionary<string, HabitTypes> typeMap, List<HabitLogs> allLogs)
        {
            var today = DateTime.Today.AddDays(_weeklyCompletionOffset * 7);
            int daysSinceMonday = ((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek) - 1;
            var startOfWeek = today.AddDays(-daysSinceMonday);
            var startOfLastWeek = startOfWeek.AddDays(-7);

            int thisWeekPlanned = 0, thisWeekCompleted = 0;
            int lastWeekPlanned = 0, lastWeekCompleted = 0;

            var weeklyCompletionDays = new ObservableCollection<WeeklyCompletionDay>();
            var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            // Initialize weekly completion days
            for (int i = 0; i < 7; i++)
            {
                var dDate = startOfWeek.AddDays(i);
                weeklyCompletionDays.Add(new WeeklyCompletionDay
                {
                    DayName = dayNames[i],
                    Value = 0,
                    MaxValue = 0,
                    BarColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 138, 93)),
                    IsFuture = dDate.Date > DateTime.Today
                });
            }

            var logsByDate = allLogs.GroupBy(l => l.LogDate.Date).ToDictionary(g => g.Key, g => g.ToList());
            var logsByHabit = allLogs.GroupBy(l => l.HabitId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var habit in userHabits)
            {
                typeMap.TryGetValue(habit.HabitTypeId ?? "", out var type);
                var typeName = type?.Type?.ToLower() ?? "checkbox";

                // This Week & Last Week
                for (int d = 0; d < 14; d++)
                {
                    var date = startOfLastWeek.AddDays(d);
                    if (habit.CreatedDate.Date > date) continue;
                    if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;
                    bool isThisWeek = d >= 7;
                    if (isThisWeek)
                    {
                        if (date.Date <= DateTime.Today)
                        {
                            thisWeekPlanned++;
                        }
                    }
                    else lastWeekPlanned++;

                    if (isThisWeek) weeklyCompletionDays[d - 7].MaxValue++;

                    if (logsByDate.TryGetValue(date, out var dayLogs))
                    {
                        var log = dayLogs.FirstOrDefault(l => l.HabitId == habit.Id);
                        if (log != null)
                        {
                            bool isCompleted = typeName == "checkbox" ? log.IsCompleted : log.NumericValue >= habit.TargetFrequency;
                            if (isCompleted)
                            {
                                if (isThisWeek) 
                                {
                                    if (date.Date <= DateTime.Today)
                                    {
                                        thisWeekCompleted++;
                                    }
                                    weeklyCompletionDays[d - 7].Value++;
                                }
                                else lastWeekCompleted++;
                            }
                        }
                    }
                }
            }

            GrowthQuotient = thisWeekPlanned > 0 ? (int)Math.Round((double)thisWeekCompleted / thisWeekPlanned * 100) : 0;
            int lastWeekGrowth = lastWeekPlanned > 0 ? (int)Math.Round((double)lastWeekCompleted / lastWeekPlanned * 100) : 0;
            int diff = GrowthQuotient - lastWeekGrowth;
            GrowthQuotientTrend = diff >= 0 ? $"+{diff}% from last week" : $"{diff}% from last week";
            IsPositiveGrowthTrend = diff >= 0;
            WeeklyCompletionDays = weeklyCompletionDays;

            var endOfWeek = startOfWeek.AddDays(6);
            WeeklyCompletionDateRange = $"{startOfWeek:dd.MM.yyyy} - {endOfWeek:dd.MM.yyyy}";

            // Top Habits (Sort by best completion percentage all-time or last 30 days)
            var topList = new List<TopHabitViewModel>();
            foreach (var habit in userHabits)
            {
                logsByHabit.TryGetValue(habit.Id, out var hLogs);
                hLogs ??= new List<HabitLogs>();
                typeMap.TryGetValue(habit.HabitTypeId ?? "", out var type);
                var typeName = type?.Type?.ToLower() ?? "checkbox";

                int sched = 0, comp = 0;
                for (int d = 0; d < 30; d++)
                {
                    var date = today.AddDays(-d);
                    if (habit.CreatedDate.Date > date) continue;
                    if (!DailyScoreCalculator.IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;
                    sched++;
                    var log = hLogs.FirstOrDefault(l => l.LogDate.Date == date);
                    if (log != null && (typeName == "checkbox" ? log.IsCompleted : log.NumericValue >= habit.TargetFrequency))
                        comp++;
                }

                int perc = sched > 0 ? (int)Math.Round((double)comp / sched * 100) : 0;
                if (perc > 0)
                {
                    topList.Add(new TopHabitViewModel
                    {
                        Name = habit.Name,
                        CompletionPercentage = perc,
                        IconPath = habit.Icon,
                        IconFgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 138, 93)),
                        IconBgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 248, 240))
                    });
                }
            }

            TopHabits = new ObservableCollection<TopHabitViewModel>(topList.OrderByDescending(h => h.CompletionPercentage).Take(4));

            // Milestones
            int totalCompletions = allLogs.Count(l => l.IsCompleted || l.NumericValue > 0); // Simplified total completion check
            Milestones = new ObservableCollection<MilestoneViewModel>
            {
                new MilestoneViewModel
                {
                    Title = "Total Completions",
                    CurrentValue = totalCompletions,
                    TargetValue = totalCompletions < 100 ? 100 : (totalCompletions < 500 ? 500 : 1000),
                    IconPath = "M12,2L15.09,8.26L22,9.27L17,14.14L18.18,21.02L12,17.77L5.82,21.02L7,14.14L2,9.27L8.91,8.26L12,2Z",
                    IconFgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 245, 158, 11)),
                    IconBgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 245, 158, 11))
                },
                new MilestoneViewModel
                {
                    Title = "Consistency King",
                    CurrentValue = GrowthQuotient,
                    TargetValue = 100,
                    IconPath = "M13,2.05V5.08C16.39,5.57 19,8.47 19,12C19,15.53 16.39,18.43 13,18.92V21.95C18.05,21.43 22,17.18 22,12C22,6.82 18.05,2.57 13,2.05M11,2.05C5.95,2.57 2,6.82 2,12C2,17.18 5.95,21.43 11,21.95V18.92C7.61,18.43 5,15.53 5,12C5,8.47 7.61,5.57 11,5.08V2.05M12,7A5,5 0 0,0 7,12A5,5 0 0,0 12,17A5,5 0 0,0 17,12A5,5 0 0,0 12,7Z",
                    IconFgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 59, 130, 246)),
                    IconBgColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 59, 130, 246))
                }
            };
        }

        // ===== Daily Note Auto-Save Methods =====

        /// <summary>
        /// Resetuje/startuje debounce timer – zapis nastąpi po 1.5s bez zmian.
        /// </summary>
        private void DebounceSaveNote()
        {
            if (_noteSaveDebounceTimer == null)
            {
                _noteSaveDebounceTimer = new DispatcherTimer();
                _noteSaveDebounceTimer.Interval = TimeSpan.FromMilliseconds(1500);
                _noteSaveDebounceTimer.Tick += async (s, e) =>
                {
                    _noteSaveDebounceTimer.Stop();
                    await SaveDailyNoteAsync();
                };
            }
            _noteSaveDebounceTimer.Stop();
            _noteSaveDebounceTimer.Start();
        }

        /// <summary>
        /// Ładuje notatkę na dziś z Supabase.
        /// </summary>
        public async Task LoadDailyNoteAsync()
        {
            // Reset stanu notatki (czyści starą notatkę np. po zmianie konta)
            _currentNoteId = null;
            _dailyNoteContent = "";
            DailyNoteStatusText = "";
            OnPropertyChanged(nameof(DailyNoteContent));

            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var todayStr = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                var response = await SupabaseService.Client.From<Notes>()
                    .Filter("user_id", Constants.Operator.Equals, userId)
                    .Filter("note_date", Constants.Operator.Equals, todayStr)
                    .Get();
                var note = response.Models?.FirstOrDefault();
                if (note != null)
                {
                    _currentNoteId = note.Id;
                    _dailyNoteContent = note.Content ?? "";
                    OnPropertyChanged(nameof(DailyNoteContent));
                }
                else
                {
                    _currentNoteId = null;
                    _dailyNoteContent = "";
                    OnPropertyChanged(nameof(DailyNoteContent));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading daily note: {ex.Message}");
            }
        }

        /// <summary>
        /// Upsert notatki dziennej do Supabase (insert lub update).
        /// </summary>
        private async Task SaveDailyNoteAsync()
        {
            var userId = SupabaseService.Client?.Auth?.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var todayDate = DateTime.UtcNow.Date;

                if (!string.IsNullOrEmpty(_currentNoteId))
                {
                    // UPDATE istniejącej notatki
                    var note = new Notes
                    {
                        Id = _currentNoteId,
                        UserId = userId,
                        NoteDate = todayDate,
                        Content = DailyNoteContent,
                        CreatedDate = todayDate
                    };
                    await SupabaseService.Client.From<Notes>()
                        .Filter("id", Constants.Operator.Equals, _currentNoteId)
                        .Update(note);
                }
                else
                {
                    // INSERT nowej notatki
                    var note = new Notes
                    {
                        UserId = userId,
                        NoteDate = todayDate,
                        Content = DailyNoteContent,
                        CreatedDate = DateTime.UtcNow
                    };
                    var result = await SupabaseService.Client.From<Notes>().Insert(note);
                    _currentNoteId = result.Models?.FirstOrDefault()?.Id;
                }

                var loc = Services.LocalizationService.Instance;
                DailyNoteStatusText = $"{loc.Get("Autozapisano o", "Auto-saved at")} {DateTime.Now:HH:mm}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving daily note: {ex.Message}");
                var loc = Services.LocalizationService.Instance;
                DailyNoteStatusText = loc.Get("Błąd zapisu", "Save error");
            }
        }
    }
}