//Plik odpowiedzialny za pobieranie nawyków i podawanie ich dalej
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using HabitTracker.Models;
using HabitTracker.Services;

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
        
    }
}