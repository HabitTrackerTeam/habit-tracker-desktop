using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTracker.Services;
using HabitTracker.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace HabitTracker.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        //Zmienne
        private const string ColorError = "#FFD32F2F";
        private const string ColorSuccess = "#FF328A5D";
        private const string ColorInfo = "#FF8B9AA2";

        private string _email = string.Empty;
        private string _nickname = string.Empty;
        private string _statusMessage = string.Empty;
        private string _statusColor = ColorInfo;
        private string _avatarPath = string.Empty;
        private string _resetToken = string.Empty;

        private bool _isLoginVisible = true;
        private bool _isRegisterVisible = false;
        private bool _isForgotVisible = false;
        private bool _isForgotEmailStep = true;
        private bool _isForgotCodeStep = false;
        private bool _isForgotNewPasswordStep = false;
        private bool _isDashboardVisible = false;
        private bool _isLoading = false;

        private bool _hasMinLength;
        private bool _hasUppercase;
        private bool _hasLowercase;
        private bool _hasDigit;
        private bool _hasSpecialChar;
        private bool _isPasswordChecklistVisible;
        

        //Wlasciwosci
        //konta do wyswietlenia
        private ObservableCollection<SavedAccount> _savedAccounts = new ObservableCollection<SavedAccount>();
        public bool HasSavedAccounts => SavedAccounts.Count>0;
        public ObservableCollection<SavedAccount> SavedAccounts { get => _savedAccounts; set { _savedAccounts = value; OnPropertyChanged(); } }

        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public string StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }
        public string AvatarPath { get => _avatarPath; set { _avatarPath = value; OnPropertyChanged(); } }
        public string ResetToken { get => _resetToken; set { _resetToken = value; OnPropertyChanged(); } }
        
        public bool IsLoginVisible { get => _isLoginVisible; set { _isLoginVisible = value; OnPropertyChanged(); } }
        public bool IsRegisterVisible { get => _isRegisterVisible; set { _isRegisterVisible = value; OnPropertyChanged(); } }
        public bool IsForgotVisible { get => _isForgotVisible; set { _isForgotVisible = value; OnPropertyChanged(); } }
        public bool IsForgotEmailStep { get => _isForgotEmailStep; set { _isForgotEmailStep = value; OnPropertyChanged(); } }
        public bool IsForgotCodeStep { get => _isForgotCodeStep; set { _isForgotCodeStep = value; OnPropertyChanged(); } }
        public bool IsForgotNewPasswordStep { get => _isForgotNewPasswordStep; set { _isForgotNewPasswordStep = value; OnPropertyChanged(); } }
        public bool IsDashboardVisible{get=>_isDashboardVisible; set{_isDashboardVisible = value; OnPropertyChanged();OnPropertyChanged(nameof(IsAuthVisible));}}
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        public bool IsAuthVisible => !IsDashboardVisible;

        public bool HasMinLength { get => _hasMinLength; set { _hasMinLength = value; OnPropertyChanged(); } }
        public bool HasUppercase { get => _hasUppercase; set { _hasUppercase = value; OnPropertyChanged(); } }
        public bool HasLowercase { get => _hasLowercase; set { _hasLowercase = value; OnPropertyChanged(); } }
        public bool HasDigit { get => _hasDigit; set { _hasDigit = value; OnPropertyChanged(); } }
        public bool HasSpecialChar { get => _hasSpecialChar; set { _hasSpecialChar = value; OnPropertyChanged(); } }
        public bool IsPasswordValid => HasMinLength && HasUppercase && HasLowercase && HasDigit && HasSpecialChar;
        public bool IsPasswordChecklistVisible { get => _isPasswordChecklistVisible; set { _isPasswordChecklistVisible = value; OnPropertyChanged(); } }

        //KONSTRUKTOR
        public LoginViewModel()
        {
            var accounts = LocalAccountService.LoadSavedAccounts();
            SavedAccounts = new ObservableCollection<SavedAccount>(accounts);

            OnPropertyChanged(nameof(HasSavedAccounts));

            if (SavedAccounts.Count > 0)
            {
                ShowAccountSelection();
            }
            else
            {
                ShowLogin();
            }
        }
        private void SetStatus(string message, string color = ColorInfo)
        {
            StatusMessage = message;
            StatusColor = color;
        }

        private void SwitchMainView(bool login, bool register, bool forgot, bool dashboard)
        {
            IsLoginVisible = login;
            IsRegisterVisible = register;
            IsForgotVisible = forgot;
            IsDashboardVisible = dashboard;
            SetStatus(string.Empty);
        }


        public void ShowLogin()
        {
            SwitchMainView(true, false, false, false);
            Nickname = string.Empty;
            AvatarPath = string.Empty;
        }

        public void ShowRegister() => SwitchMainView(false, true, false, false);

        public void ShowForgot()
        {
            SwitchMainView(false, false, true, false);
            IsForgotEmailStep = true;
            IsForgotCodeStep = false;
            IsForgotNewPasswordStep = false;
            ResetToken = string.Empty;
        }

        public void ShowAccountSelection() => SwitchMainView(true, false, false, false);

        private bool ValidateBasicInput(string password)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                SetStatus("Email can't be empty", ColorError);
                return false;
            }
            if (!Email.Contains("@"))
            {
                SetStatus("Wrong email format", ColorError);
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                SetStatus("Password can't be empty", ColorError);
                return false;
            }
            return true;
        }
        public void ValidatePassword(string password)
        {
            HasMinLength = password.Length >= 6;
            HasUppercase = password.Any(char.IsUpper);
            HasLowercase = password.Any(char.IsLower);
            HasDigit = password.Any(char.IsDigit);
            HasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));
            OnPropertyChanged(nameof(IsPasswordValid));
        }

        public void ResetPasswordChecklist()
        {
            HasMinLength = false;
            HasUppercase = false;
            HasLowercase = false;
            HasDigit = false;
            HasSpecialChar = false;
            IsPasswordChecklistVisible = false;
            OnPropertyChanged(nameof(IsPasswordValid));
        }

        public async Task<bool> RegisterAsync(string password)
        {
            if (!ValidateBasicInput(password)) return false;

            if (!IsPasswordValid)
            {
                SetStatus("Password does not meet all requirements.", ColorError);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                SetStatus("Nickname is required.", ColorError);
                return false;
            }

            SetStatus("Processing your registration...");

            try
            {
                string finalAvatarUrl = string.Empty;

                if (!string.IsNullOrEmpty(AvatarPath))
                {
                    SetStatus("Uploading avatar...");
                    byte[] imageBytes = System.IO.File.ReadAllBytes(AvatarPath);
                    string extension = System.IO.Path.GetExtension(AvatarPath);
                    string uniqueFileName = $"{Guid.NewGuid()}{extension}";

                    await SupabaseService.Client.Storage.From("avatars").Upload(imageBytes, uniqueFileName);
                    finalAvatarUrl = SupabaseService.Client.Storage.From("avatars").GetPublicUrl(uniqueFileName);
                }

                SetStatus("Creating account...");

                var signUpOptions = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "nickname", Nickname },
                        { "avatar_url", finalAvatarUrl }
                    }
                };

                var session = await SupabaseService.Client.Auth.SignUp(Email, password, signUpOptions);

                if (session?.User != null)
                {
                    var newAccount = new SavedAccount
                    {
                        Email = Email,
                        Nickname = Nickname,
                        AvatarUrl = finalAvatarUrl,
                        LastLogin = DateTime.Now
                    };
                    LocalAccountService.SaveAccount(newAccount);

                    SetStatus("Account created successfully!", ColorSuccess);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message.Contains("User already registered") ? "This email is already taken." : "Error occurred during registration.", ColorError);
                return false;
            }
        }

        public async Task<bool> LoginAsync(string password)
        {
            if (!ValidateBasicInput(password)) return false;

            try
            {
                var session = await SupabaseService.Client.Auth.SignIn(Email, password);
                if (session?.User != null)
                {
                    string savedNickname = session.User.UserMetadata != null && session.User.UserMetadata.ContainsKey("nickname") ? session.User.UserMetadata["nickname"].ToString() : Email;

                    string savedAvatar = session.User.UserMetadata != null && session.User.UserMetadata.ContainsKey("avatar_url") ? session.User.UserMetadata["avatar_url"].ToString() : "";

                    //zapis/update konta na dysku
                    var loggedAccount = new SavedAccount
                    {
                        Email = Email,
                        Nickname = savedNickname,
                        AvatarUrl = savedAvatar,
                        LastLogin = DateTime.Now
                    };
                    LocalAccountService.SaveAccount(loggedAccount);

                    SetStatus($"Hello {savedNickname}", ColorSuccess);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message.Contains("invalid_credentials") ? "Invalid email or password." : "An error occurred. Please try again.", ColorError);
                return false;
            }
        }

        public async Task ResetPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                SetStatus("Please enter a valid email address.", ColorError);
                return;
            }

            SetStatus("Sending reset code...");

            try
            {
                await SupabaseService.Client.Auth.ResetPasswordForEmail(Email);
                SetStatus("Code sent! Check your inbox.", ColorSuccess);

                IsForgotEmailStep = false;
                IsForgotCodeStep = true;
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", ColorError);
            }
        }

        public async Task VerifyCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(ResetToken) || ResetToken.Length < 6)
            {
                SetStatus("Enter the valid code.", ColorError);
                return;
            }

            SetStatus("Verifying code...");
            try
            {
                var session = await SupabaseService.Client.Auth.VerifyOTP(Email, ResetToken, Supabase.Gotrue.Constants.EmailOtpType.Recovery);

                if (session?.User != null)
                {
                    SetStatus("Code verified!", ColorSuccess);
                    IsForgotCodeStep = false;
                    IsForgotNewPasswordStep = true;
                }
            }
            catch (Exception)
            {
                SetStatus("Invalid code. Please try again.", ColorError);
            }
        }

        public async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            if (!IsPasswordValid)
            {
                SetStatus("Password does not meet all requirements.", ColorError);
                return false;
            }

            SetStatus("Updating password...");
            try
            {
                var attrs = new Supabase.Gotrue.UserAttributes { Password = newPassword };
                await SupabaseService.Client.Auth.Update(attrs);

                SetStatus("Password updated successfully!", ColorSuccess);
                await Task.Delay(1500);
                ShowLogin();
                return true;
            }
            catch (Exception)
            {
                SetStatus("Error occurred while updating password.", ColorError);
                return false;
            }
        }
        public void ShowDashboard()=>SwitchMainView(false,false,false,true);
    }
}