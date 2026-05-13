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
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HabitTracker.ViewModels{
    public class DashboardViewModel:ViewModelBase{
        //Lista automatycznie odswiezajaca XAML, gdy pojawia sie w niej nowe dane
        private ObservableCollection<Habits> _habits = new ObservableCollection<Habits>();
        public ObservableCollection<HabitCategory> Categories {get;set;} = new();
        public ObservableCollection<HabitTypes> HabitTypes {get;set;} = new();
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

        private bool _isPdfReportVisible = false;
        public bool IsPdfReportVisible
        {
            get => _isPdfReportVisible;
            set { _isPdfReportVisible = value; OnPropertyChanged(); }
        }

        private bool _isDashboardContent = true;
        public bool IsDashboardContent
        {
            get => _isDashboardContent;
            set { _isDashboardContent = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BodyParts> BodyParts { get; set; } = new();
        public ObservableCollection<CircumferenceLogs> MeasurementLogs { get; set; } = new();

        public ObservableCollection<BodyParts> AvailableBodyParts { get; set; } = new();
        public ObservableCollection<MeasurementItemViewModel> CurrentSessionMeasurements { get; set; } = new();

        public ObservableCollection<BodyMetricItem> BodyMetrics { get; set; } = new();
        private BodyMetricItem _selectedBodyMetric;
        public BodyMetricItem SelectedBodyMetric
        {
            get => _selectedBodyMetric;
            set 
            { 
                _selectedBodyMetric = value; 
                OnPropertyChanged();

                if (_selectedBodyMetric != null)
                {
                    UpdateChartDataAsync(_selectedBodyMetric.BodyPartId);
                }
            }
        }

        private double _currentWeight;
        public double CurrentWeight { get => _currentWeight; set { _currentWeight = value; OnPropertyChanged(); } }
        private double _weightDelta;
        public double WeightDelta { get => _weightDelta; set { _weightDelta = value; OnPropertyChanged(); } }
        private double _currentHeight;
        public double CurrentHeight { get => _currentHeight; set { _currentHeight = value; OnPropertyChanged(); } }
        private double _bmiValue;
        public double BmiValue { get => _bmiValue; set { _bmiValue = value; OnPropertyChanged(); } }
        private string _bmiStatus = "";
        public string BmiStatus { get => _bmiStatus; set { _bmiStatus = value; OnPropertyChanged(); } }

        private double? _newWeightValue;
        public double? NewWeightValue { get => _newWeightValue; set { _newWeightValue = value; OnPropertyChanged(); } }

        private double? _newHeightValue;
        public double? NewHeightValue { get => _newHeightValue; set { _newHeightValue = value; OnPropertyChanged(); } }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set { _isModalOpen = value; OnPropertyChanged(); }
        }
        public ICommand OpenLogModalCommand { get; }
        public ICommand CloseLogModalCommand { get; }

        public ISeries[] ChartSeries { get; set; } = new ISeries[0];
        public Axis[] XAxes { get; set; } = new Axis[0];
        public Axis[] YAxes { get; set; } = new Axis[0];

        private BodyParts _selectedBodyPartToAdd;
        public BodyParts SelectedBodyPartToAdd
        {
            get => _selectedBodyPartToAdd;
            set
            {
                _selectedBodyPartToAdd = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddMeasurementCommand { get; }
        public ICommand RemoveMeasurementCommand { get; }

        private DateTime _measurementDate = DateTime.Now;
        public DateTime MeasurementDate
        {
            get => _measurementDate;
            set { _measurementDate = value; OnPropertyChanged(); }
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
            AddMeasurementCommand = new RelayCommand(ExecuteAddMeasurement, CanExecuteAddMeasurement);
            RemoveMeasurementCommand = new RelayCommand(ExecuteRemoveMeasurement);
            OpenLogModalCommand = new RelayCommand(_ => IsModalOpen = true);
            CloseLogModalCommand = new RelayCommand(_ => IsModalOpen = false);
            PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            SetMonthlyViewCommand = new RelayCommand(_ => IsMonthlyView = true);
            SetWeeklyViewCommand = new RelayCommand(_ => IsMonthlyView = false);
            GenerateCalendar();
            // Initialize today's date and start timer to update it every minute
            UpdateTodayDate();
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) => UpdateTodayDate();
            timer.Start();
        }

        private bool CanExecuteAddMeasurement(object obj)
        {
            return SelectedBodyPartToAdd != null;
        }

        private void ExecuteAddMeasurement(object obj)
        {
            if (SelectedBodyPartToAdd != null)
            {
                var bodyPart = SelectedBodyPartToAdd;

                CurrentSessionMeasurements.Add(new MeasurementItemViewModel
                {
                    BodyPartId = bodyPart.Id,
                    BodyPartName = bodyPart.DisplayName
                });

                AvailableBodyParts.Remove(bodyPart);
                SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();
            }
        }

        private void ExecuteRemoveMeasurement(object obj)
        {
            if (obj is MeasurementItemViewModel item)
            {
                CurrentSessionMeasurements.Remove(item);

                var bodyPart = BodyParts.FirstOrDefault(bp => bp.Id == item.BodyPartId);
                if (bodyPart != null)
                {
                    AvailableBodyParts.Add(bodyPart);
                }

                if (SelectedBodyPartToAdd == null)
                {
                    SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();
                }
            }
        }

        public async Task LoadMeasurementsAsync()
        {
            try
            {
                IsLoading = true;
                var parts = await SupabaseService.Client.From<BodyParts>().Get();
                BodyParts = new ObservableCollection<BodyParts>(parts.Models);
                OnPropertyChanged(nameof(BodyParts));

                AvailableBodyParts.Clear();
                foreach(var part in BodyParts)
                {
                    AvailableBodyParts.Add(part);
                }
                SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();

                CurrentSessionMeasurements.Clear();

                var logs = await SupabaseService.Client.From<CircumferenceLogs>().Get();
                MeasurementLogs = new ObservableCollection<CircumferenceLogs>(logs.Models);
                OnPropertyChanged(nameof(MeasurementLogs));

                // Process logs to populate BodyMetrics
                BodyMetrics.Clear();
                foreach (var part in BodyParts)
                {
                    var partLogs = MeasurementLogs.Where(l => l.BodyPartId == part.Id)
                                                  .OrderByDescending(l => l.Session?.MeasurementDate).ToList();

                    if (partLogs.Any())
                    {
                        var latest = partLogs.First().Value;
                        double previous = partLogs.Count > 1 ? partLogs[1].Value : latest;
                        double delta = latest - previous;

                        BodyMetrics.Add(new BodyMetricItem
                        {
                            BodyPartId = part.Id,
                            PartName = part.DisplayName,
                            LatestValue = latest,
                            Delta = delta,
                            IsPositiveTrend = delta > 0
                        });
                    }
                }

                if (BodyMetrics.Any() && SelectedBodyMetric == null)
                {
                    SelectedBodyMetric = BodyMetrics.First();
                }

                // Temporary weight and height logic
                var bodyMetricsDb = await SupabaseService.Client.From<HabitTracker.Models.BodyMetrics>().Get();
                var latestBodyMetric = bodyMetricsDb.Models.OrderByDescending(m => m.MeasurementDate).FirstOrDefault();
                if (latestBodyMetric != null)
                {
                    CurrentWeight = latestBodyMetric.Weight;
                    CurrentHeight = latestBodyMetric.Height;

                    var metricsRecords = bodyMetricsDb.Models.OrderByDescending(m => m.MeasurementDate).ToList();
                    if(metricsRecords.Count > 1) 
                    {
                        WeightDelta = CurrentWeight - metricsRecords[1].Weight;
                    }

                    CalculateBmi();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load measurements: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateBmi()
        {
            if (CurrentHeight > 0)
            {
                var heightInMeters = CurrentHeight / 100.0;
                BmiValue = Math.Round(CurrentWeight / (heightInMeters * heightInMeters), 1);

                if (BmiValue < 18.5) BmiStatus = "Underweight";
                else if (BmiValue < 25) BmiStatus = "Normal";
                else if (BmiValue < 30) BmiStatus = "Overweight";
                else BmiStatus = "Obese";
            }
        }

        private async void UpdateChartDataAsync(string bodyPartId)
        {
            try
            {
                var logs = await SupabaseService.Client.From<CircumferenceLogs>()
                    .Where(l => l.BodyPartId == bodyPartId)
                    .Get();

                var sortedLogs = logs.Models
                    .Where(l => l.Session != null)
                    .OrderBy(l => l.Session.MeasurementDate)
                    .ToList();

                var values = new List<double>();
                var labels = new List<string>();

                // Add tooltip logic by formatting the X axis string more precisely, 
                // but setting labels allows tooltips to inherit that text in default LV2 configurations
                foreach (var log in sortedLogs)
                {
                    values.Add(log.Value);
                    // Use exact date for the label so the tooltip shows exactly when it happened
                    labels.Add(log.Session.MeasurementDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                }

                ChartSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Name = "Measurement",
                        Fill = new SolidColorPaint(new SKColor(50, 138, 93).WithAlpha(50)), // Green-ish transparent fill
                        Stroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 3 },
                        GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255)),
                        GeometryStroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 2 }
                    }
                };
                OnPropertyChanged(nameof(ChartSeries));

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsPaint = new SolidColorPaint(SKColors.Gray),
                        TextSize = 12
                    }
                };
                OnPropertyChanged(nameof(XAxes));

                YAxes = new Axis[]
                {
                    new Axis
                    {
                        LabelsPaint = new SolidColorPaint(SKColors.Transparent), // Hide Y Axis labels if we want it clean like the UI
                        SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1, PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 3, 3 }) }
                    }
                };
                OnPropertyChanged(nameof(YAxes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating chart: {ex.Message}");
            }
        }

        public async Task SaveMeasurementAsync()
        {
            if (!CurrentSessionMeasurements.Any() && !NewWeightValue.HasValue && !NewHeightValue.HasValue)
            {
                System.Windows.MessageBox.Show("Please add at least one measurement, weight, or height to the session.");
                return;
            }

            var validItems = CurrentSessionMeasurements.Where(i => i.Value.HasValue && i.Value.Value > 0).ToList();
            if (validItems.Count != CurrentSessionMeasurements.Count)
            {
                System.Windows.MessageBox.Show("All added body part measurements must have a valid value greater than 0 before saving.");
                return;
            }

            try
            {
                IsLoading = true;

                // Save Body Metrics (Weight/Height) directly to BodyMetrics table if provided
                if (NewWeightValue.HasValue || NewHeightValue.HasValue)
                {
                    double w = NewWeightValue ?? CurrentWeight; // default to existing if omitted
                    double h = NewHeightValue ?? CurrentHeight;

                    var bmEntry = new HabitTracker.Models.BodyMetrics
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = SupabaseService.Client.Auth.CurrentUser.Id,
                        MeasurementDate = MeasurementDate,
                        Weight = w,
                        Height = h,
                        AdditionalNotes = ""
                    };
                    await SupabaseService.Client.From<HabitTracker.Models.BodyMetrics>().Insert(bmEntry);

                    // Clear inputs after success
                    NewWeightValue = null;
                    NewHeightValue = null;
                }

                // Proceed with existing circumferences setup only if there are items
                if (validItems.Any())
                {
                    var userId = SupabaseService.Client.Auth.CurrentUser.Id;
                    var today = MeasurementDate.Date;

                    // 1. Fetch or Create Session
                    var existingSessions = await SupabaseService.Client.From<MeasurementSessions>()
                        .Where(s => s.UserId == userId && s.MeasurementDate == today)
                        .Get();

                    string sessionId;
                    if (existingSessions.Models.Any())
                    {
                        // Session exists
                        sessionId = existingSessions.Models.First().Id;
                    }
                    else
                    {
                        // Create new session
                        var session = new MeasurementSessions
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            MeasurementDate = today,
                            AdditionalNotes = ""
                        };
                        var sessionResponse = await SupabaseService.Client.From<MeasurementSessions>().Insert(session);
                        sessionId = sessionResponse.Models.FirstOrDefault()?.Id;
                    }

                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        // 2. Upsert circumference logs
                        foreach (var item in validItems)
                        {
                            var log = new CircumferenceLogs
                            {
                                Id = Guid.NewGuid().ToString(),
                                SessionId = sessionId,
                                BodyPartId = item.BodyPartId,
                                Value = item.Value.Value
                            };

                            var options = new Supabase.Postgrest.QueryOptions 
                            { 
                                DuplicateResolution = Supabase.Postgrest.QueryOptions.DuplicateResolutionType.MergeDuplicates,
                                OnConflict = "session_id, body_part_id"
                            };

                            await SupabaseService.Client.From<CircumferenceLogs>().Upsert(log, options);
                        }

                        CurrentSessionMeasurements.Clear();

                        AvailableBodyParts.Clear();
                        foreach (var part in BodyParts)
                        {
                            AvailableBodyParts.Add(part);
                        }
                        SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();

                        MeasurementDate = DateTime.Now;
                    }
                } // This closes: if (validItems.Any())

                await LoadMeasurementsAsync();
                System.Windows.MessageBox.Show("Measurements saved successfully!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving measurement: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SwitchToMeasurements()
        {
            IsHabitsVisible = false;
            IsMeasurementsVisible = true;
            IsAddFormVisible = false;
            IsDashboardContent = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
            IsPdfReportVisible = false;
            _ = LoadMeasurementsAsync();
        }

        public void SwitchToHabits()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = true;
            IsAddFormVisible = true;
            IsDashboardContent = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
            IsPdfReportVisible = false;
        }

        public void SwitchToDashboard()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = true;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
            IsPdfReportVisible = false;
        }

        public void SwitchToSettings()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = false;
            IsSettingsVisible = true;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
            IsPdfReportVisible = false;
        }

        public void SwitchToCalendar()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = false;
            IsSettingsVisible = false;
            IsCalendarVisible = true;
            IsStatisticsVisible = false;
            IsPdfReportVisible = false;
        }

        public void SwitchToStatistics()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = true;
            IsPdfReportVisible = false;
        }

        public void SwitchToPdfReport()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = false;
            IsSettingsVisible = false;
            IsCalendarVisible = false;
            IsStatisticsVisible = false;
            IsPdfReportVisible = true;
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