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

        private bool _isMonthlyView = true;
        public bool IsMonthlyView
        {
            get => _isMonthlyView;
            set { _isMonthlyView = value; OnPropertyChanged(); }
        }

        public ICommand SetMonthlyViewCommand { get; }
        public ICommand SetWeeklyViewCommand { get; }

        private DateTime _currentCalendarDate = DateTime.Now;

        public DashboardViewModel()
        {
            PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            SetMonthlyViewCommand = new RelayCommand(_ => IsMonthlyView = true);
            SetWeeklyViewCommand = new RelayCommand(_ => IsMonthlyView = false);
            GenerateCalendar();
            
            // Initialize filter options
            InitializeFilters();
            
            // Initialize today's date and start timer to update it every minute
            UpdateTodayDate();
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) => UpdateTodayDate();
            timer.Start();
        }

        private void InitializeFilters()
        {
            // Priorities
            Priorities.Add(new FilterItem { Id = "all", Name = "All" });
            Priorities.Add(new FilterItem { Id = "1", Name = "High" });
            Priorities.Add(new FilterItem { Id = "2", Name = "Medium" });
            Priorities.Add(new FilterItem { Id = "3", Name = "Low" });
            SelectedPriority = Priorities.First();

            // Statuses
            Statuses.Add(new FilterItem { Id = "all", Name = "All" });
            Statuses.Add(new FilterItem { Id = "active", Name = "Active" });
            Statuses.Add(new FilterItem { Id = "archived", Name = "Archived" });
            SelectedStatus = Statuses.First(s => s.Id == "active"); // Default to Active

            // Frequencies
            Frequencies.Add(new FilterItem { Id = "all", Name = "All" });
            Frequencies.Add(new FilterItem { Id = "daily", Name = "Daily" });
            Frequencies.Add(new FilterItem { Id = "weekly", Name = "Weekly" });
            Frequencies.Add(new FilterItem { Id = "monthly", Name = "Monthly" });
            Frequencies.Add(new FilterItem { Id = "specific", Name = "Specific" });
            SelectedFrequency = Frequencies.First();
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

                List<HabitLogs> todayLogs = new List<HabitLogs>();
                try
                {
                    var todayStart = DateTime.UtcNow.Date;
                    var todayEnd = todayStart.AddDays(1);
                    var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                        .Filter("log_date", Constants.Operator.GreaterThanOrEqual, todayStart)
                        .Filter("log_date", Constants.Operator.LessThan, todayEnd)
                        .Get();
                    if (logsResponse.Models != null)
                    {
                        todayLogs = logsResponse.Models;
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

                    var log = todayLogs.FirstOrDefault(l => l.HabitId == habit.Id);
                    if (log != null)
                    {
                        habit.IsCompleted = log.IsCompleted;
                        habit.CurrentProgress = log.NumericValue;
                    }
                    else
                    {
                        habit.IsCompleted = false;
                        habit.CurrentProgress = 0;
                    }

                    // Wire up PropertyChanged to save progress to DB
                    habit.PropertyChanged -= Habit_PropertyChanged;
                    habit.PropertyChanged += Habit_PropertyChanged;
                }

                Habits = new ObservableCollection<Habits>(habitsList);
                
                // Default view used by HomeTab (only show active habits)
                var defaultView = CollectionViewSource.GetDefaultView(Habits);
                defaultView.Filter = (obj) => 
                {
                    if (obj is Habits h) return !h.IsArchived;
                    return true;
                };

                // Independent view for Habit Manager
                FilteredHabits = new CollectionViewSource { Source = Habits }.View;
                FilteredHabits.Filter = FilterHabit;
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
            }
        }

        public async Task SaveHabitLogAsync(Habits habit)
        {
            try
            {
                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);
                
                var logsResponse = await SupabaseService.Client.From<HabitLogs>()
                    .Filter("habit_id", Constants.Operator.Equals, habit.Id)
                    .Filter("log_date", Constants.Operator.GreaterThanOrEqual, todayStart)
                    .Filter("log_date", Constants.Operator.LessThan, todayEnd)
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
                System.Windows.MessageBox.Show("Please select a habit from the list.");
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
                System.Windows.MessageBox.Show("Please enter a habit name.");
                return;
            }

            try{
                IsLoading = true;

                // Look up HabitType UUID by display name
                var habitType = HabitTypes.FirstOrDefault(t => 
                    string.Equals(t.DisplayType, NewHabitType, StringComparison.OrdinalIgnoreCase));
                
                if (habitType == null)
                {
                    System.Windows.MessageBox.Show($"Habit type '{NewHabitType}' not found in database.");
                    return;
                }

                if (EditingHabit != null)
                {
                    EditingHabit.Name = NewHabitName;
                    EditingHabit.HabitTypeId = habitType.Id;
                    EditingHabit.Icon = NewHabitIcon;
                    EditingHabit.Period = NewHabitFrequency;
                    EditingHabit.TargetFrequency = (int)NewHabitGoal;
                    EditingHabit.DaysOfWeek = NewHabitDaysOfWeek;
                    EditingHabit.Priority = NewHabitPriority;
                    EditingHabit.Unit = NewHabitUnit;

                    await SupabaseService.Client.From<Habits>()
                        .Filter("id", Constants.Operator.Equals, EditingHabit.Id)
                        .Update(EditingHabit);

                    System.Windows.MessageBox.Show("Habit updated successfully! ✏️");
                }
                else
                {
                    var habit = new Habits{
                        Name = NewHabitName,
                        HabitTypeId = habitType.Id,
                        UserId = SupabaseService.Client.Auth.CurrentUser.Id,
                        Icon = NewHabitIcon,
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

                    System.Windows.MessageBox.Show("Habit created successfully! 🌱");
                }

                //Reset formularza i refresh listy
                ResetAddHabitForm();
                IsAddHabitModalOpen = false;
                await LoadHabitsAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving habit: {ex.Message}");
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
                System.Windows.MessageBox.Show($"Error archiving habit: {ex.Message}");
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
            TodayDate = DateTime.Now.ToString("yyyy-MM-dd, dddd", CultureInfo.GetCultureInfo("en-US"));
        }

        private void ChangeMonth(int monthsToAdd)
        {
            _currentCalendarDate = _currentCalendarDate.AddMonths(monthsToAdd);
            GenerateCalendar();
        }

        private void GenerateCalendar()
        {
            CurrentMonthYear = _currentCalendarDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("en-US"));
            var days = new ObservableCollection<CalendarDayViewModel>();
            
            var firstDayOfMonth = new DateTime(_currentCalendarDate.Year, _currentCalendarDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentCalendarDate.Year, _currentCalendarDate.Month);
            
            // Assuming week starts on Monday
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (startDayOfWeek == 0) startDayOfWeek = 7; // Sunday is 7
            
            var startDate = firstDayOfMonth.AddDays(-(startDayOfWeek - 1));
            var rnd = new Random();
            
            // We usually need 35 or 42 days to fill a calendar grid
            int totalDays = 42; 
            if (startDayOfWeek == 1 && daysInMonth == 28) totalDays = 28;
            else if (startDayOfWeek + daysInMonth - 1 <= 35) totalDays = 35;
            
            for (int i = 0; i < totalDays; i++)
            {
                var date = startDate.AddDays(i);
                bool isCurrentMonth = date.Month == _currentCalendarDate.Month;
                bool isToday = date.Date == DateTime.Today;
                
                int percentage = rnd.Next(40, 100); 
                if (rnd.Next(0, 10) > 8) percentage = 0;
                if (isToday) percentage = 100;

                string badgeColor = "#E6F4EA"; // Light green
                string dotColor = "#328A5D"; // Green dot
                
                if (percentage < 50) 
                {
                    badgeColor = "#FFF0F0"; // Light red
                    dotColor = "#D32F2F"; // Red dot
                }
                if (percentage == 0)
                {
                    badgeColor = "#F0F2F5"; // Gray
                    dotColor = "Transparent"; 
                }

                if(isToday)
                {
                    badgeColor = "#4CA475"; // Darker green for today
                    dotColor = "#FFFFFF"; // White dot
                }

                days.Add(new CalendarDayViewModel
                {
                    Date = date,
                    DayNumber = date.ToString("dd"),
                    PercentageText = isCurrentMonth ? $"{percentage}%" : "",
                    IsCurrentMonth = isCurrentMonth,
                    IsToday = isToday,
                    IsSelected = isToday,
                    BadgeColor = badgeColor,
                    DotColor = dotColor
                });
            }
            
            CalendarDays = days;
        }
    }
}