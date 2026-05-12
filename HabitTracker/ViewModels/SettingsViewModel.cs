using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public ObservableCollection<string> AvailableLanguages { get; } = new()
        {
            "Polski", "English", "Deutsch"
        };

        public ObservableCollection<string> AvailableWeekStarts { get; } = new()
        {
            "Poniedziałek", "Niedziela"
        };

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
            set { _selectedLanguage = value; OnPropertyChanged(); }
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

        private string _selectedTheme;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set { _selectedTheme = value; OnPropertyChanged(); }
        }

        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
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
            set { _userAvatarUrl = value; OnPropertyChanged(); }
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

                await UserSettingsService.SaveSettingsAsync(_currentSettings);

                StatusMessage = "Settings saved successfully!";
                StatusColor = "#FF328A5D";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
                StatusColor = "#FFD32F2F";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ApplySettingsToUI(UserSettings settings)
        {
            SelectedLanguage = settings.Language switch
            {
                "pl" => "Polski",
                "en" => "English",
                "de" => "Deutsch",
                _ => "Polski"
            };

            SelectedWeekStart = settings.WeekStart switch
            {
                "monday" => "Poniedziałek",
                "sunday" => "Niedziela",
                _ => "Poniedziałek"
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
            SelectedTheme = settings.Theme;
        }

        private void ApplyUIToSettings(UserSettings settings)
        {
            settings.Language = SelectedLanguage switch
            {
                "Polski" => "pl",
                "English" => "en",
                "Deutsch" => "de",
                _ => "pl"
            };

            settings.WeekStart = SelectedWeekStart switch
            {
                "Poniedziałek" => "monday",
                "Niedziela" => "sunday",
                _ => "monday"
            };

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
            settings.Theme = SelectedTheme;
        }
    }
}
