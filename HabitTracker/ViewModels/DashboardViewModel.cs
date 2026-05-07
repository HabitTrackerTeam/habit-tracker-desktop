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

        public DashboardViewModel()
        {
            AddMeasurementCommand = new RelayCommand(ExecuteAddMeasurement, CanExecuteAddMeasurement);
            RemoveMeasurementCommand = new RelayCommand(ExecuteRemoveMeasurement);
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

        public async Task SaveMeasurementAsync()
        {
            if (!CurrentSessionMeasurements.Any())
            {
                System.Windows.MessageBox.Show("Please add at least one measurement to the session.");
                return;
            }

            var validItems = CurrentSessionMeasurements.Where(i => i.Value.HasValue && i.Value.Value > 0).ToList();
            if (validItems.Count != CurrentSessionMeasurements.Count)
            {
                System.Windows.MessageBox.Show("All added measurements must have a valid value greater than 0 before saving.");
                return;
            }

            try
            {
                IsLoading = true;
                var session = new MeasurementSessions
                {
                    UserId = SupabaseService.Client.Auth.CurrentUser.Id,
                    MeasurementDate = MeasurementDate,
                    AdditionalNotes = ""
                };
                var sessionResponse = await SupabaseService.Client.From<MeasurementSessions>().Insert(session);
                var createdSession = sessionResponse.Models.FirstOrDefault();

                if (createdSession != null)
                {
                    foreach (var item in validItems)
                    {
                        var log = new CircumferenceLogs
                        {
                            SessionId = createdSession.Id,
                            BodyPartId = item.BodyPartId,
                            Value = item.Value.Value
                        };
                        await SupabaseService.Client.From<CircumferenceLogs>().Insert(log);
                    }

                    CurrentSessionMeasurements.Clear();

                    AvailableBodyParts.Clear();
                    foreach (var part in BodyParts)
                    {
                        AvailableBodyParts.Add(part);
                    }
                    SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();

                    MeasurementDate = DateTime.Now;

                    await LoadMeasurementsAsync();
                    System.Windows.MessageBox.Show("Measurements saved successfully!");
                }
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
            _ = LoadMeasurementsAsync();
        }

        public void SwitchToHabits()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = true;
            IsAddFormVisible = true;
            IsDashboardContent = false;
        }

        public void SwitchToDashboard()
        {
            IsMeasurementsVisible = false;
            IsHabitsVisible = false;
            IsAddFormVisible = false;
            IsDashboardContent = true;
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
    }
}