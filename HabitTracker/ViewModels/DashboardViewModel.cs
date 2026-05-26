//Plik odpowiedzialny za pobieranie nawyków i podawanie ich dalej
using System;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Windows.Input;
using System.Windows.Threading;
using HabitTracker.Commands;
using System.Collections.Generic;

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
        public ObservableCollection<HabitCategory> Categories {get;set;} = new();
        public ObservableCollection<HabitTypes> HabitTypes {get;set;} = new();
        
        // Filter collections
        public ObservableCollection<FilterItem> Priorities { get; set; } = new();
        public ObservableCollection<FilterItem> Statuses { get; set; } = new();
        public ObservableCollection<FilterItem> Frequencies { get; set; } = new();

        // Selected filters
        private FilterItem _selectedPriority;
        public FilterItem SelectedPriority { get => _selectedPriority; set { _selectedPriority = value; OnPropertyChanged(); } }

        private FilterItem _selectedStatus;
        public FilterItem SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); } }

        private FilterItem _selectedFrequency;
        public FilterItem SelectedFrequency { get => _selectedFrequency; set { _selectedFrequency = value; OnPropertyChanged(); } }

        private HabitCategory _selectedCategoryFilter;
        public HabitCategory SelectedCategoryFilter { get => _selectedCategoryFilter; set { _selectedCategoryFilter = value; OnPropertyChanged(); } }

        //Pola zbierajace dane z formularza
        private string _newHabitName;
        public string NewHabitName{get=>_newHabitName; set{_newHabitName=value; OnPropertyChanged();}}

        private HabitCategory _selectedCategory;
        public HabitCategory SelectedCategory {get=>_selectedCategory; set{_selectedCategory=value; OnPropertyChanged();}}

        private HabitTypes _selectedType;
        public HabitTypes SelectedType {get=>_selectedType; set{_selectedType = value; OnPropertyChanged();}}

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

        private bool _isCreateCategoryModalOpen = false;
        public bool IsCreateCategoryModalOpen
        {
            get => _isCreateCategoryModalOpen;
            set { _isCreateCategoryModalOpen = value; OnPropertyChanged(); }
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
            Priorities.Add(new FilterItem { Id = "all", Name = "All Priorities" });
            Priorities.Add(new FilterItem { Id = "1", Name = "High" });
            Priorities.Add(new FilterItem { Id = "2", Name = "Medium" });
            Priorities.Add(new FilterItem { Id = "3", Name = "Low" });
            SelectedPriority = Priorities.First();

            // Statuses
            Statuses.Add(new FilterItem { Id = "all", Name = "Active Habits" });
            Statuses.Add(new FilterItem { Id = "active", Name = "Active Only" });
            Statuses.Add(new FilterItem { Id = "archived", Name = "Archived" });
            SelectedStatus = Statuses.First();

            // Frequencies
            Frequencies.Add(new FilterItem { Id = "all", Name = "Every Day" });
            Frequencies.Add(new FilterItem { Id = "daily", Name = "Daily" });
            Frequencies.Add(new FilterItem { Id = "weekly", Name = "Weekly" });
            Frequencies.Add(new FilterItem { Id = "monthly", Name = "Monthly" });
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
                //pobranie nawykow z DB
                var response = await SupabaseService.Client.From<Habits>().Get();
                //update listy na ekranie
                Habits = new ObservableCollection<Habits>(response.Models);
            }
            catch(Exception ex){
                SetStatus($"Failed to download habits: {ex.Message}","FFD32F2F");
            }
            finally{
                IsLoading=false;
            }
        }

        //Metoda ladujaca dane
        public async Task LoadFormDataAsync(){
            try{
                var categories = await SupabaseService.Client.From<HabitCategory>().Get();
                Categories = new ObservableCollection<HabitCategory>(categories.Models);
                OnPropertyChanged(nameof(Categories));

                var types = await SupabaseService.Client.From<HabitTypes>().Get();
                HabitTypes = new ObservableCollection<HabitTypes>(types.Models);
                OnPropertyChanged(nameof(HabitTypes));

                SelectedCategory = Categories.FirstOrDefault();
                SelectedType = HabitTypes.FirstOrDefault();

            }
            catch(Exception ex){
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
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

            // Ensure there is a category/type selected
            if (SelectedCategory == null) SelectedCategory = Categories.FirstOrDefault();
            if (SelectedType == null) SelectedType = HabitTypes.FirstOrDefault();

            await CreateHabitAsync();
        }

        //Dodawanie nawyku
        public async Task CreateHabitAsync(){
            if(string.IsNullOrWhiteSpace(NewHabitName) || SelectedCategory == null || SelectedType == null){
                System.Windows.MessageBox.Show("Fullfill all fields!");
                return;
            }

            try{
                IsLoading = true;

                var habit = new Habits{
                    Name = NewHabitName,
                    CategoryId=SelectedCategory.Id,
                    HabitTypeId=SelectedType.Id,
                    //Pobranie id z sesji zalogowanego uzytkownika
                    UserId = SupabaseService.Client.Auth.CurrentUser.Id,
                    Period = "Daily",
                    TargetFrequency = 1,
                    DaysOfWeek  = 127,
                    Priority = 1,
                    IsFlexible = false
                };
                await SupabaseService.Client.From<Habits>().Insert(habit);

                //Reset formularza i refresh listy
                NewHabitName = string.Empty;
                IsAddFormVisible = false;
                await LoadHabitsAsync();

                System.Windows.MessageBox.Show("Habit added successfully!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
                
            }
            finally{IsLoading = false;}
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