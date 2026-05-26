using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.ViewModels;

public class AddHabitViewModel : ViewModelBase
{
    private DashboardViewModel _parentVM;

    // Form Fields
    private string _habitName;
    public string HabitName 
    { 
        get => _habitName; 
        set { _habitName = value; OnPropertyChanged(); } 
    }

    private int _habitTypeId = 1; // Default: Numeric
    public int HabitTypeId 
    { 
        get => _habitTypeId; 
        set { _habitTypeId = value; OnPropertyChanged(); } 
    }

    private int _priority = 2; // Default: Medium (1=High, 2=Medium, 3=Low)
    public int Priority 
    { 
        get => _priority; 
        set { _priority = value; OnPropertyChanged(); } 
    }



    private string _selectedIcon = "✓";
    public string SelectedIcon 
    { 
        get => _selectedIcon; 
        set { _selectedIcon = value; OnPropertyChanged(); } 
    }

    private string _selectedColor = "#3CAEA3";
    public string SelectedColor 
    { 
        get => _selectedColor; 
        set { _selectedColor = value; OnPropertyChanged(); } 
    }

    private string _frequency = "Daily"; // Daily, Weekly, Monthly, Specific
    public string Frequency 
    { 
        get => _frequency; 
        set { _frequency = value; OnPropertyChanged(); } 
    }

    private string _daysOfWeek = "1111100"; // M-T-W-T-F-S-S
    public string DaysOfWeek 
    { 
        get => _daysOfWeek; 
        set { _daysOfWeek = value; OnPropertyChanged(); } 
    }

    private double _goalValue = 20;
    public double GoalValue 
    { 
        get => _goalValue; 
        set { _goalValue = value; OnPropertyChanged(); } 
    }

    private string _unit = "mins";
    public string Unit 
    { 
        get => _unit; 
        set { _unit = value; OnPropertyChanged(); } 
    }

    // Collections

    public ObservableCollection<string> HabitTypes { get; set; } = new();
    public ObservableCollection<string> Units { get; set; } = new();

    private bool _isLoading;
    public bool IsLoading 
    { 
        get => _isLoading; 
        set { _isLoading = value; OnPropertyChanged(); } 
    }

    public AddHabitViewModel(DashboardViewModel parentVM = null)
    {
        _parentVM = parentVM;
        LoadFormData();
    }

    private async void LoadFormData()
    {
        try
        {
            IsLoading = true;

            // Load from parent VM if available (future use)
            if (_parentVM != null)
            {
                // Parent VM available
            }

            // Populate units
            Units.Add("mins");
            Units.Add("hours");
            Units.Add("pages");
            Units.Add("count");
            Units.Add("custom");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load form data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> SaveHabitAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(HabitName))
            {
                System.Windows.MessageBox.Show("Please enter a habit name.");
                return false;
            }



            IsLoading = true;

            // Map frequency to period string
            int targetFrequency = 1;
            int daysOfWeekInt = 127; // All days
            string period = Frequency;

            if (Frequency == "Weekly")
            {
                daysOfWeekInt = Convert.ToInt32(DaysOfWeek, 2);
                targetFrequency = 1;
            }
            else if (Frequency == "Monthly")
            {
                targetFrequency = 1;
            }

            var habit = new Habits
            {
                Name = HabitName,
                HabitTypeId = _habitTypeId.ToString(), // Need to store as string for FK
                UserId = SupabaseService.Client.Auth.CurrentUser?.Id,
                Period = period,
                TargetFrequency = targetFrequency,
                DaysOfWeek = daysOfWeekInt,
                Priority = _priority,
                IsFlexible = true,
                IsArchived = false,
                IsSystem = false,
                CreatedDate = DateTime.UtcNow
            };

            // Insert into database
            await SupabaseService.Client.From<Habits>().Insert(habit);

            // Refresh parent's habit list
            if (_parentVM != null)
            {
                await _parentVM.LoadHabitsAsync();
            }

            System.Windows.MessageBox.Show("Habit created successfully! 🌱");
            return true;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error saving habit: {ex.Message}");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
