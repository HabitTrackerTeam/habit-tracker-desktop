using System.Windows;
using HabitTracker.Models;
using HabitTracker.ViewModels;
using System.Linq;

namespace HabitTracker.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private readonly DashboardViewModel _dashboardVM;
    private readonly MeasurementsViewModel _measurementsVM;

    public DashboardView()
    {
        _dashboardVM = new DashboardViewModel();
        _measurementsVM = new MeasurementsViewModel();

        InitializeComponent();

        MainContentArea.DataContext = _dashboardVM;
        MeasurementsViewControl.DataContext = _measurementsVM;

        Loaded += DashboardView_Loaded;
        this.IsVisibleChanged += DashboardView_IsVisibleChanged;
    }

    private async void InnerGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.IsVisible && _dashboardVM != null)
        {
            await _dashboardVM.LoadFormDataAsync();
            await _dashboardVM.LoadHabitsAsync();
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        ViewModel.ShowAccountSelection();
    }

    public void UpdateThemeToggleVisuals(bool isDark)
    {
        // Settings tab owns the theme toggle now.
    }

    public void SyncThemeToggle(bool isDark)
    {
        // Settings tab owns the theme toggle now.
    }

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.LoadFormDataAsync();
        await _dashboardVM.LoadHabitsAsync();
        UpdateSidebar(NavHome);
    }

    private async void DashboardView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (this.IsVisible)
        {
            await _dashboardVM.LoadFormDataAsync();
            await _dashboardVM.LoadHabitsAsync();
        }
    }

    private void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("AddHabitModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Visible;
        }
    }

    private void CloseAddHabitModal_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("AddHabitModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Collapsed;
        }
    }

    private void CancelAddHabit_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("AddHabitModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Collapsed;
        }
    }

    private async void PlantHabit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _dashboardVM.CreateHabitAsync();
            if (FindName("AddHabitModal") is System.Windows.Controls.Grid modal)
            {
                modal.Visibility = Visibility.Collapsed;
            }
            await _dashboardVM.LoadHabitsAsync();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Wystąpił błąd podczas zapisywania nawyku: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TypeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            var typeButtons = new System.Windows.Controls.Button?[]
            {
                FindName("TypeNumericBtn") as System.Windows.Controls.Button,
                FindName("TypeCheckboxBtn") as System.Windows.Controls.Button,
                FindName("TypeTimerBtn") as System.Windows.Controls.Button
            };
            ResetButtonGroup(typeButtons, btn);
        }
    }

    private void PriorityButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            var priorityButtons = new System.Windows.Controls.Button?[]
            {
                FindName("PriorityLowBtn") as System.Windows.Controls.Button,
                FindName("PriorityMediumBtn") as System.Windows.Controls.Button,
                FindName("PriorityHighBtn") as System.Windows.Controls.Button
            };
            ResetButtonGroup(priorityButtons, btn);
        }
    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            var bgBrush = btn.Background as System.Windows.Media.SolidColorBrush;
            bool isSelected = bgBrush == null || bgBrush.Color.R == 255;

            var green = System.Windows.Media.Color.FromArgb(255, 50, 138, 93);
            var gray = System.Windows.Media.Color.FromArgb(255, 221, 226, 229);
            var darkGray = System.Windows.Media.Color.FromArgb(255, 78, 96, 108);

            if (isSelected)
            {
                btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 248, 240));
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(green);
                btn.BorderThickness = new System.Windows.Thickness(2);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(green);
            }
            else
            {
                btn.Background = System.Windows.Media.Brushes.White;
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(gray);
                btn.BorderThickness = new System.Windows.Thickness(1);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(darkGray);
            }
        }
    }



    private void ResetButtonGroup(System.Windows.Controls.Button?[] buttons, System.Windows.Controls.Button selectedBtn)
    {
        var green = System.Windows.Media.Color.FromArgb(255, 50, 138, 93);
        var gray = System.Windows.Media.Color.FromArgb(255, 221, 226, 229);
        var darkGray = System.Windows.Media.Color.FromArgb(255, 78, 96, 108);

        foreach (var btn in buttons)
        {
            if (btn == null) continue;

            if (btn == selectedBtn)
            {
                btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 248, 240));
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(green);
                btn.BorderThickness = new System.Windows.Thickness(2);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(green);
            }
            else
            {
                btn.Background = System.Windows.Media.Brushes.White;
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(gray);
                btn.BorderThickness = new System.Windows.Thickness(1);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(darkGray);
            }
        }
    }



    private void ChooseBuiltIn_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.IsBuiltInMode = true;
    }

    private void ChooseCustom_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.IsBuiltInMode = false;
    }

    private async void AddBuiltIn_Click(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.AddBuiltInHabitAsync();
    }

    private void SidebarNewHabit_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.IsAddFormVisible = !_dashboardVM.IsAddFormVisible;
    }

    private void SwitchToSettings_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToSettings();
        UpdateSidebar(NavSettings);
    }

    private void SwitchToCalendar_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToCalendar();
        UpdateSidebar(NavCalendar);
    }

    private void SwitchToStatistics_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToStatistics();
        UpdateSidebar(NavStatistics);
    }

    private void SwitchToHome_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToHome();
        UpdateSidebar(NavHome);
    }

    private async void SaveHabit_Click(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.CreateHabitAsync();
    }

    private void EditHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
        {
            _dashboardVM.NewHabitName = habit.Name ?? string.Empty;
            var type = _dashboardVM.HabitTypes?.FirstOrDefault(t => t.Id == habit.HabitTypeId);
            if (type != null) _dashboardVM.SelectedType = type;
            _dashboardVM.IsAddFormVisible = true;
        }
    }

    private void DeleteHabit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
        {
            var res = MessageBox.Show($"Are you sure you want to deactivate '{habit.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                _dashboardVM.Habits.Remove(habit);
            }
        }
    }

    private void SwitchToHabits_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToHabits();
        if (sender is System.Windows.Controls.Button btn && (btn == NavHome || btn == NavHabits))
        {
            UpdateSidebar(btn);
        }
        else
        {
            UpdateSidebar(NavHome);
        }
    }

    private async void SwitchToMeasurements_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToMeasurements();
        UpdateSidebar(NavMeasurements);
        await _measurementsVM.LoadMeasurementsAsync();
    }

    private void UpdateSidebar(System.Windows.Controls.Button activeBtn)
    {
        var buttons = new[] { NavHome, NavHabits, NavCalendar, NavStatistics, NavMeasurements, NavSettings };
        foreach (var b in buttons)
        {
            if (b == null) continue;
            b.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
            b.ClearValue(System.Windows.Controls.Button.BorderThicknessProperty);
            b.ClearValue(System.Windows.Controls.Button.BorderBrushProperty);
            if (b.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                if (sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                if (sp.Children[1] is System.Windows.Controls.TextBlock tb2)
                {
                    tb2.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                    tb2.ClearValue(System.Windows.Controls.TextBlock.FontWeightProperty);
                }
            }
        }

        if (activeBtn != null)
        {
            activeBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECFDF5"));
            activeBtn.BorderThickness = new System.Windows.Thickness(0, 0, 4, 0);
            activeBtn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"));
            
            if (activeBtn.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                var greenBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#047857"));
                if (sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.Foreground = greenBrush;
                if (sp.Children[1] is System.Windows.Controls.TextBlock tb2)
                {
                    tb2.Foreground = greenBrush;
                    tb2.FontWeight = FontWeights.Bold;
                }
            }
        }
    }
}
