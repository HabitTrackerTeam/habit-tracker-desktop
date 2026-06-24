using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HabitTracker.Commands;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private UserSettings _currentSettings;
        private string _statusMessage = string.Empty;
        private string _statusColor = "#FF8B9AA2";
        private bool _isSaving;
        private bool _isApplyingSettings;

        public ObservableCollection<string> AvailableLanguages { get; } = new()
        {
            "Polski", "English"
        };

        public ObservableCollection<string> AvailableWeekStarts
        {
            get
            {
                var loc = Services.LocalizationService.Instance;
                return new ObservableCollection<string> { loc.Monday, loc.Sunday };
            }
        }

        public ObservableCollection<string> AvailableUnits { get; } = new()
        {
            "kg / ml", "lb / oz"
        };

        public ObservableCollection<string> AvailableReminderTimes { get; } = new()
        {
            "08:00", "12:00", "20:00"
        };

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set 
            { 
                _selectedLanguage = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvailableWeekStarts));
                
                // Keep the selection valid if the language changes
                if (_selectedWeekStart == "Monday" || _selectedWeekStart == "Poniedziałek")
                    SelectedWeekStart = Services.LocalizationService.Instance.Monday;
                else if (_selectedWeekStart == "Sunday" || _selectedWeekStart == "Niedziela")
                    SelectedWeekStart = Services.LocalizationService.Instance.Sunday;

                // Language is applied only when user clicks Save Settings
            }
        }

        private string _selectedWeekStart;
        public string SelectedWeekStart
        {
            get => _selectedWeekStart;
            set { _selectedWeekStart = value; OnPropertyChanged(); }
        }

        private string _selectedUnits;
        public string SelectedUnits
        {
            get => _selectedUnits;
            set { _selectedUnits = value; OnPropertyChanged(); }
        }

        private bool _isDailyReminder;
        public bool IsDailyReminder
        {
            get => _isDailyReminder;
            set { _isDailyReminder = value; OnPropertyChanged(); }
        }

        private string _selectedReminderTime;
        public string SelectedReminderTime
        {
            get => _selectedReminderTime;
            set { _selectedReminderTime = value; OnPropertyChanged(); }
        }

        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get => _isSoundEnabled;
            set { _isSoundEnabled = value; OnPropertyChanged(); }
        }

        private bool _isVibrationEnabled;
        public bool IsVibrationEnabled
        {
            get => _isVibrationEnabled;
            set { _isVibrationEnabled = value; OnPropertyChanged(); }
        }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set 
            { 
                if (_isDarkMode == value) return;
                _isDarkMode = value; 
                OnPropertyChanged(); 
                // Theme is applied only when user clicks Save Settings
            }
        }

        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set 
            { 
                _userName = value; 
                OnPropertyChanged();
                UserInitial = !string.IsNullOrEmpty(value) ? value[0].ToString().ToUpper() : "?";
            }
        }

        private bool _isWeeklyReportsEnabled;
        public bool IsWeeklyReportsEnabled
        {
            get => _isWeeklyReportsEnabled;
            set { _isWeeklyReportsEnabled = value; OnPropertyChanged(); }
        }

        private bool _isAchievementBadgesEnabled;
        public bool IsAchievementBadgesEnabled
        {
            get => _isAchievementBadgesEnabled;
            set { _isAchievementBadgesEnabled = value; OnPropertyChanged(); }
        }

        private bool _isPublicProfileEnabled;
        public bool IsPublicProfileEnabled
        {
            get => _isPublicProfileEnabled;
            set { _isPublicProfileEnabled = value; OnPropertyChanged(); }
        }

        private bool _isShareProgressEnabled = true;
        public bool IsShareProgressEnabled
        {
            get => _isShareProgressEnabled;
            set { _isShareProgressEnabled = value; OnPropertyChanged(); }
        }

        private string _userEmail = string.Empty;
        public string UserEmail
        {
            get => _userEmail;
            set { _userEmail = value; OnPropertyChanged(); }
        }

        private string _userAvatarUrl = string.Empty;
        public string UserAvatarUrl
        {
            get => _userAvatarUrl;
            set 
            { 
                _userAvatarUrl = value; 
                OnPropertyChanged();
                LoadAvatarImage(value);
            }
        }

        private BitmapImage? _avatarImageSource;
        public BitmapImage? AvatarImageSource
        {
            get => _avatarImageSource;
            set { _avatarImageSource = value; OnPropertyChanged(); }
        }

        public bool HasAvatarImage => AvatarImageSource != null;

        private void LoadAvatarImage(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                AvatarImageSource = null;
                OnPropertyChanged(nameof(HasAvatarImage));
                return;
            }

            try
            {
                // Add cache-busting if not already present
                var imageUrl = url;
                if (!imageUrl.Contains("?t="))
                {
                    imageUrl = $"{imageUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.DecodePixelWidth = 120;
                bitmap.EndInit();
                AvatarImageSource = bitmap;
            }
            catch
            {
                AvatarImageSource = null;
            }
            OnPropertyChanged(nameof(HasAvatarImage));
        }

        private string _userInitial = "?";
        public string UserInitial
        {
            get => _userInitial;
            set { _userInitial = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); }
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            SaveSettingsCommand = new RelayCommand(async _ => await SaveSettingsAsync(), _ => !IsSaving);
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                var currentUser = SupabaseService.Client.Auth.CurrentUser;
                if (currentUser == null) return;

                UserEmail = currentUser.Email ?? string.Empty;

                if (currentUser.UserMetadata != null)
                {
                    UserAvatarUrl = currentUser.UserMetadata.ContainsKey("avatar_url")
                        ? currentUser.UserMetadata["avatar_url"].ToString()
                        : string.Empty;
                }

                _currentSettings = await UserSettingsService.LoadSettingsAsync(currentUser.Id);
                ApplySettingsToUI(_currentSettings);

                if (string.IsNullOrEmpty(UserName))
                {
                    if (currentUser.UserMetadata != null && currentUser.UserMetadata.ContainsKey("nickname"))
                    {
                        UserName = currentUser.UserMetadata["nickname"].ToString() ?? string.Empty;
                    }
                    else
                    {
                        UserName = currentUser.Email ?? string.Empty;
                    }
                }

                UserInitial = !string.IsNullOrEmpty(UserName) ? UserName[0].ToString().ToUpper() : "?";

                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading settings: {ex.Message}";
                StatusColor = "#FFD32F2F";
            }
        }

        public async Task SaveSettingsAsync()
        {
            if (_currentSettings == null) return;

            try
            {
                IsSaving = true;
                StatusMessage = "Saving settings...";
                StatusColor = "#FF8B9AA2";

                ApplyUIToSettings(_currentSettings);

                // Apply language to live UI
                var langCode = SelectedLanguage switch
                {
                    "English" => "en",
                    _ => "pl"
                };
                LocalizationService.Instance.CurrentLanguage = langCode;

                // Apply theme to live UI
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.ApplyTheme(IsDarkMode);
                    }
                });

                await UserSettingsService.SaveSettingsAsync(_currentSettings);

                StatusMessage = LocalizationService.Instance.SaveSettings + " ✓";
                StatusColor = "#FF166534";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusColor = "#FFD32F2F";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ApplySettingsToUI(UserSettings settings)
        {
            _isApplyingSettings = true;
            try
            {
                SelectedLanguage = settings.Language switch
                {
                    "pl" => "Polski",
                    "en" => "English",
                    _ => "Polski"
                };

                SelectedWeekStart = settings.WeekStart switch
                {
                    "monday" => Services.LocalizationService.Instance.Monday,
                    "sunday" => Services.LocalizationService.Instance.Sunday,
                    _ => Services.LocalizationService.Instance.Monday
                };

                SelectedUnits = settings.Units switch
                {
                    "metric" => "kg / ml",
                    "imperial" => "lb / oz",
                    _ => "kg / ml"
                };

                UserName = settings.Nickname;
                IsDailyReminder = settings.DailyReminder;
                SelectedReminderTime = settings.ReminderTime;
                IsSoundEnabled = settings.SoundEnabled;
                IsVibrationEnabled = settings.VibrationEnabled;
                IsDarkMode = settings.Theme == "dark";
            }
            finally
            {
                _isApplyingSettings = false;
            }
        }

        private void ApplyUIToSettings(UserSettings settings)
        {
            settings.Language = SelectedLanguage switch
            {
                "Polski" => "pl",
                "English" => "en",
                _ => "pl"
            };

            var loc = Services.LocalizationService.Instance;
            
            settings.WeekStart = SelectedWeekStart == loc.Monday || SelectedWeekStart == "Poniedziałek" || SelectedWeekStart == "Monday" ? "monday" : "sunday";

            settings.Units = SelectedUnits switch
            {
                "kg / ml" => "metric",
                "lb / oz" => "imperial",
                _ => "metric"
            };

            settings.Nickname = UserName;
            settings.DailyReminder = IsDailyReminder;
            settings.ReminderTime = SelectedReminderTime;
            settings.SoundEnabled = IsSoundEnabled;
            settings.VibrationEnabled = IsVibrationEnabled;
            settings.Theme = IsDarkMode ? "dark" : "light";
        }
    }
}
