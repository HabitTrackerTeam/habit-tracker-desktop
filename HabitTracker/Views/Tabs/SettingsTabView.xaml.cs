using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HabitTracker.ViewModels;
using Supabase.Gotrue;

namespace HabitTracker.Views.Tabs
{
    public partial class SettingsTabView : UserControl
    {
        private SettingsViewModel _settingsVM;

        public SettingsTabView()
        {
            InitializeComponent();
            _settingsVM = new SettingsViewModel();
            DataContext = _settingsVM;
            Loaded += SettingsTabView_Loaded;
            IsVisibleChanged += SettingsTabView_IsVisibleChanged;
        }

        private async void SettingsTabView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                if (_settingsVM != null)
                {
                    await _settingsVM.LoadSettingsAsync();
                }
            }
        }

        private async void SettingsTabView_Loaded(object sender, RoutedEventArgs e)
        {
            await _settingsVM.LoadSettingsAsync();
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsVM != null)
            {
                await _settingsVM.SaveSettingsAsync();
            }
        }

        private async void ChangePassword_Click(object sender, MouseButtonEventArgs e)
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
                        await HabitTracker.Services.SupabaseService.Client.Auth.SignIn(email, passwordWindow.OldPassword);

                        var attrs = new UserAttributes { Password = passwordWindow.NewPassword };
                        await HabitTracker.Services.SupabaseService.Client.Auth.Update(attrs);
                        MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("User email not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
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
                _settingsVM?.UserName ?? string.Empty,
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

                if (!string.IsNullOrEmpty(editProfileWindow.SelectedPhotoPath))
                {
                    await UploadAvatarAsync(editProfileWindow.SelectedPhotoPath);
                }
            }
            DialogOverlay.Visibility = Visibility.Collapsed;
        }

        private async void DirectChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select an Avatar Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;
                DialogOverlay.Visibility = Visibility.Visible;
                await UploadAvatarAsync(selectedPath);
                DialogOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task UploadAvatarAsync(string localPath)
        {
            try
            {
                var currentUser = HabitTracker.Services.SupabaseService.Client.Auth.CurrentUser;
                if (currentUser != null)
                {
                    var fileBytes = System.IO.File.ReadAllBytes(localPath);
                    var extension = System.IO.Path.GetExtension(localPath).ToLowerInvariant();
                    var fileName = $"{currentUser.Id}/avatar{extension}";
                    var contentType = extension switch
                    {
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".bmp" => "image/bmp",
                        _ => "image/jpeg"
                    };

                    await HabitTracker.Services.SupabaseService.Client.Storage
                        .From("avatars")
                        .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                        {
                            ContentType = contentType,
                            Upsert = true
                        });

                    var baseUrl = HabitTracker.Services.SupabaseService.Client.Storage
                        .From("avatars")
                        .GetPublicUrl(fileName);
                    var publicUrl = $"{baseUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                    var attrs = new UserAttributes();
                    attrs.Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "avatar_url", baseUrl }
                    };
                    await HabitTracker.Services.SupabaseService.Client.Auth.Update(attrs);

                    if (_settingsVM != null)
                    {
                        _settingsVM.UserAvatarUrl = publicUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload photo:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
