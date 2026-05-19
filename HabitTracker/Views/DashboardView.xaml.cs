using System.Windows;
using HabitTracker.ViewModels;
using System.Linq;
using HabitTracker.Models;
using HabitTracker.Views;

namespace HabitTracker.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private DashboardViewModel _dashboardVM;
    private SettingsViewModel _settingsVM;
    private MeasurementsViewModel _measurementsVM;
    public DashboardView()
    {
        InitializeComponent();

        _dashboardVM=new DashboardViewModel();
        _settingsVM = new SettingsViewModel();
        _measurementsVM = new MeasurementsViewModel();

        MainContentArea.DataContext = _dashboardVM;
        SettingsPanel.DataContext = _settingsVM;
        MeasurementsViewControl.DataContext = _measurementsVM;

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
        await _settingsVM.LoadSettingsAsync(); // This applies language and theme
        await _dashboardVM.LoadFormDataAsync();
        await _dashboardVM.LoadHabitsAsync();
        UpdateSidebar(NavDashboard);
    }

    private async void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        var addHabitWindow = new AddHabitWindow(_dashboardVM);
        addHabitWindow.Owner = Window.GetWindow(this);
        if (addHabitWindow.ShowDialog() == true)
        {
            // Dialog closed successfully, refresh list
            await _dashboardVM.LoadHabitsAsync();
        }
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

    private async void EditProfile_Click(object sender, RoutedEventArgs e)
    {
        DialogOverlay.Visibility = Visibility.Visible;
        var editProfileWindow = new EditProfileWindow(
            _settingsVM?.UserName ?? "",
            _settingsVM?.UserAvatarUrl);
        editProfileWindow.Owner = Window.GetWindow(this);
        if (editProfileWindow.ShowDialog() == true)
        {
            if (_settingsVM != null)
            {
                _settingsVM.UserName = editProfileWindow.NewFullName;
                _settingsVM.UserInitial = !string.IsNullOrEmpty(editProfileWindow.NewFullName) 
                    ? editProfileWindow.NewFullName[0].ToString().ToUpper() : "?";
            }

            // Handle photo upload if a new photo was selected
            if (!string.IsNullOrEmpty(editProfileWindow.SelectedPhotoPath))
            {
                try
                {
                    var currentUser = HabitTracker.Services.SupabaseService.Client.Auth.CurrentUser;
                    if (currentUser != null)
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(editProfileWindow.SelectedPhotoPath);
                        var extension = System.IO.Path.GetExtension(editProfileWindow.SelectedPhotoPath).ToLowerInvariant();
                        var fileName = $"{currentUser.Id}/avatar{extension}";
                        var contentType = extension switch
                        {
                            ".png" => "image/png",
                            ".gif" => "image/gif",
                            ".bmp" => "image/bmp",
                            _ => "image/jpeg"
                        };

                        // Upload to Supabase Storage (avatars bucket) with upsert
                        await HabitTracker.Services.SupabaseService.Client.Storage
                            .From("avatars")
                            .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                            {
                                ContentType = contentType,
                                Upsert = true
                            });

                        // Get the public URL with cache-busting timestamp
                        var baseUrl = HabitTracker.Services.SupabaseService.Client.Storage
                            .From("avatars")
                            .GetPublicUrl(fileName);
                        var publicUrl = $"{baseUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                        // Update user metadata with the avatar URL
                        var attrs = new Supabase.Gotrue.UserAttributes();
                        attrs.Data = new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "avatar_url", baseUrl }
                        };
                        await HabitTracker.Services.SupabaseService.Client.Auth.Update(attrs);

                        // Immediately update the UI
                        if (_settingsVM != null)
                        {
                            _settingsVM.UserAvatarUrl = publicUrl;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to upload photo:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        DialogOverlay.Visibility = Visibility.Collapsed;
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

    private void EditHabit_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
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
        if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
        {
            var res = System.Windows.MessageBox.Show($"Are you sure you want to deactivate '{habit.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                _dashboardVM.Habits.Remove(habit);
                // TODO: call backend to mark habit as inactive/persist change
            }
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

    private async void SwitchToMeasurements_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToMeasurements();
            UpdateSidebar(NavMeasurements);
            await _measurementsVM.LoadMeasurementsAsync();
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
