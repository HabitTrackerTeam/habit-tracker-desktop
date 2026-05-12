using System.Windows;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private DashboardViewModel _dashboardVM;
    private SettingsViewModel _settingsVM;
    public DashboardView()
    {
        InitializeComponent();

        _dashboardVM=new DashboardViewModel();
        _settingsVM = new SettingsViewModel();

        MainContentArea.DataContext = _dashboardVM;
        SettingsPanel.DataContext = _settingsVM;

        this.Loaded+=DashboardView_Loaded;
    }


    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        ViewModel.ShowAccountSelection();
    }

    public void UpdateThemeToggleVisuals(bool isDark)
    {
        // No longer needed as we removed the sidebar toggle icons/labels
    }

    public void SyncThemeToggle(bool isDark)
    {
        if (DarkModeToggle != null && DarkModeToggle.IsChecked != isDark)
        {
            DarkModeToggle.IsChecked = isDark;
        }
    }

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.LoadFormDataAsync();
        await _dashboardVM.LoadHabitsAsync();
        UpdateSidebar(NavDashboard);
    }

    private async void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.CreateHabitAsync();
    }

    private void ChooseBuiltIn_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.IsBuiltInMode = true;
        }
    }

    private void ChooseCustom_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.IsBuiltInMode = false;
        }
    }

    private async void AddBuiltIn_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            await _dashboardVM.AddBuiltInHabitAsync();
        }
    }

    private void SidebarNewHabit_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.IsAddFormVisible = !_dashboardVM.IsAddFormVisible;
        }
    }
    private async void SwitchToSettings_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToSettings();
            UpdateSidebar(NavSettings);
            await _settingsVM.LoadSettingsAsync();
        }
    }

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        if (_settingsVM != null)
        {
            await _settingsVM.SaveSettingsAsync();
        }
    }

    private async void ChangePassword_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DialogOverlay.Visibility = Visibility.Visible;
        var passwordWindow = new ChangePasswordWindow();
        passwordWindow.Owner = Window.GetWindow(this);
        if (passwordWindow.ShowDialog() == true)
        {
            try
            {
                var email = HabitTracker.Services.SupabaseService.Client.Auth.CurrentUser?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    // Weryfikacja starego hasła przez logowanie
                    await HabitTracker.Services.SupabaseService.Client.Auth.SignIn(email, passwordWindow.OldPassword);

                    // Aktualizacja hasła na nowe
                    var attrs = new Supabase.Gotrue.UserAttributes { Password = passwordWindow.NewPassword };
                    await HabitTracker.Services.SupabaseService.Client.Auth.Update(attrs);
                    MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("User email not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to change password. Make sure the old password is correct.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        DialogOverlay.Visibility = Visibility.Collapsed;
    }

    private void EditProfile_Click(object sender, RoutedEventArgs e)
    {
        DialogOverlay.Visibility = Visibility.Visible;
        var editProfileWindow = new EditProfileWindow(_settingsVM?.UserName ?? "");
        editProfileWindow.Owner = Window.GetWindow(this);
        if (editProfileWindow.ShowDialog() == true)
        {
            if (_settingsVM != null)
            {
                _settingsVM.UserName = editProfileWindow.NewFullName;
            }
        }
        DialogOverlay.Visibility = Visibility.Collapsed;
    }

    private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.HandleThemeToggle(sender, e);
        }
    }
    private void SwitchToCalendar_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToCalendar();
            UpdateSidebar(NavCalendar);
        }
    }

    private void SwitchToStatistics_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToStatistics();
            UpdateSidebar(NavStatistics);
        }
    }

    private void SwitchToPdfReport_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToPdfReport();
            UpdateSidebar(NavPdfReport);
        }
    }
    private void SwitchToDashboard_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToDashboard();
            UpdateSidebar(NavDashboard);
        }
    }

    private async void SaveHabit_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            await _dashboardVM.CreateHabitAsync();
        }
    }

    private void SwitchToHabits_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
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
    }

    private void SwitchToMeasurements_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToMeasurements();
            UpdateSidebar(NavMeasurements);
        }
    }
    
    private void UpdateSidebar(System.Windows.Controls.Button activeBtn)
    {
        // Reset all sidebar buttons' visuals
        var buttons = new[] { NavDashboard, NavHabits, NavCalendar, NavStatistics, NavMeasurements, NavPdfReport, NavSettings };
        foreach (var b in buttons)
        {
            if (b == null) continue;
            b.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
            if (b.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                if (sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                if (sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty); tb2.ClearValue(System.Windows.Controls.TextBlock.FontWeightProperty); }
            }
        }

        // Apply active style (Green)
        if (activeBtn != null)
        {
            activeBtn.SetResourceReference(System.Windows.Controls.Button.BackgroundProperty, "InputBgBrush");
            if (activeBtn.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                var greenBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#328A5D"));
                if (sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.Foreground = greenBrush;
                if (sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.Foreground = greenBrush; tb2.FontWeight = FontWeights.Bold; }
            }
        }
    }
}
