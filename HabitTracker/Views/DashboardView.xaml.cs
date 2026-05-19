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
        InitializeComponent();

        _dashboardVM = new DashboardViewModel();
        _measurementsVM = new MeasurementsViewModel();

        MainContentArea.DataContext = _dashboardVM;
        MeasurementsViewControl.DataContext = _measurementsVM;

        Loaded += DashboardView_Loaded;
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
        UpdateSidebar(NavDashboard);
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

    private void NewCategoryFromAddHabit_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("CreateCategoryModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Visible;
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

    private void OpenCreateCategoryModal_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("CreateCategoryModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Visible;
        }
    }

    private void CloseCreateCategoryModal_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("CreateCategoryModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Collapsed;
        }
    }

    private void CancelCreateCategory_Click(object sender, RoutedEventArgs e)
    {
        if (FindName("CreateCategoryModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Collapsed;
        }
    }

    private void CreateCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        var categoryNameBox = FindName("CategoryNameBoxCreateCategory") as System.Windows.Controls.TextBox;
        string categoryName = categoryNameBox?.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(categoryName))
        {
            MessageBox.Show("Please enter a category name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show(
            $"Category '{categoryName}' will be created!",
            "Success",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        if (FindName("CreateCategoryModal") is System.Windows.Controls.Grid modal)
        {
            modal.Visibility = Visibility.Collapsed;
        }
    }

    private void CategoryIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            UpdateIconSelection(btn);
        }
    }

    private void CategoryColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            UpdateColorSelection(btn);
        }
    }

    private void UpdateIconSelection(System.Windows.Controls.Button selectedBtn)
    {
        var children = (selectedBtn.Parent as System.Windows.Controls.Panel)?.Children;
        if (children == null) return;

        var selectedBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 138, 93));

        foreach (var child in children)
        {
            if (child is System.Windows.Controls.Button btn)
            {
                btn.Background = btn == selectedBtn ? selectedBackground : System.Windows.Media.Brushes.White;
                btn.Foreground = btn == selectedBtn ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Black;
            }
        }
    }

    private void UpdateColorSelection(System.Windows.Controls.Button selectedBtn)
    {
        var parentPanel = selectedBtn.Parent as System.Windows.Controls.StackPanel;
        if (parentPanel == null) return;

        foreach (var child in parentPanel.Children)
        {
            if (child is System.Windows.Controls.Button btn && btn.Width == 40)
            {
                if (btn == selectedBtn)
                {
                    btn.BorderThickness = new System.Windows.Thickness(3);
                    btn.BorderBrush = System.Windows.Media.Brushes.Black;
                }
                else
                {
                    btn.BorderThickness = new System.Windows.Thickness(0);
                    btn.BorderBrush = null;
                }
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

    private void SwitchToDashboard_Click(object sender, RoutedEventArgs e)
    {
        _dashboardVM.SwitchToDashboard();
        UpdateSidebar(NavDashboard);
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
            var cat = _dashboardVM.Categories?.FirstOrDefault(c => c.Id == habit.CategoryId);
            if (cat != null) _dashboardVM.SelectedCategory = cat;
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
        if (sender is System.Windows.Controls.Button btn && (btn == NavDashboard || btn == NavHabits))
        {
            UpdateSidebar(btn);
        }
        else
        {
            UpdateSidebar(NavDashboard);
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
        var buttons = new[] { NavDashboard, NavHabits, NavCalendar, NavStatistics, NavMeasurements, NavSettings };
        foreach (var b in buttons)
        {
            if (b == null) continue;
            b.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
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
            activeBtn.SetResourceReference(System.Windows.Controls.Button.BackgroundProperty, "InputBgBrush");
            if (activeBtn.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                var greenBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#328A5D"));
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
