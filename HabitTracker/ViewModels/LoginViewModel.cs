using System.ComponentModel;
using System.Runtime.CompilerServices;
using HabitTracker.Services;
namespace HabitTracker.ViewModels
{
    public class LoginViewModel : ViewModelBase // Listener for input values
    {
        private string _email = string.Empty;
        private string _nickname = string.Empty;
        private string _statusMessage = string.Empty;
        private string _statusColor = "#FF8B9AA2";

        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public string StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

        private bool _isLoginVisible = true;
        private bool _isRegisterVisible = false;
        private bool _isForgotVisible = false;

        public bool IsLoginVisible { get => _isLoginVisible; set { _isLoginVisible = value; OnPropertyChanged(); } }
        public bool IsRegisterVisible { get => _isRegisterVisible; set { _isRegisterVisible = value; OnPropertyChanged(); } }
        public bool IsForgotVisible { get => _isForgotVisible; set { _isForgotVisible = value; OnPropertyChanged(); } }

        private string _resetToken = string.Empty;
        private bool _isForgotEmailStep = true;
        private bool _isForgotCodeStep = false;
        private bool _isForgotNewPasswordStep = false;

        public string ResetToken { get => _resetToken; set { _resetToken = value; OnPropertyChanged(); } }
        public bool IsForgotEmailStep { get => _isForgotEmailStep; set { _isForgotEmailStep = value; OnPropertyChanged(); } }
        public bool IsForgotCodeStep { get => _isForgotCodeStep; set { _isForgotCodeStep = value; OnPropertyChanged(); } }
        public bool IsForgotNewPasswordStep { get => _isForgotNewPasswordStep; set { _isForgotNewPasswordStep = value; OnPropertyChanged(); } }

        public void ShowLogin()
        {
            IsLoginVisible = true; IsRegisterVisible = false; IsForgotVisible = false;
            StatusMessage = string.Empty;
        }

        public void ShowRegister()
        {
            IsLoginVisible = false; IsRegisterVisible = true; IsForgotVisible = false;
            StatusMessage = string.Empty;
        }

        public void ShowForgot()
        {
            IsLoginVisible = false; IsRegisterVisible = false; IsForgotVisible = true;
            IsForgotEmailStep = true; //Pokazujemy pole na email
            IsForgotCodeStep = false; // ukrywamy pole na PIN
            IsForgotNewPasswordStep = false;
            StatusMessage = string.Empty;
            ResetToken = string.Empty;
        }

        public bool Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                StatusColor = "#FFD32F2F";
                StatusMessage = "Email can't be empty";
                return false;
            }
            if (!Email.Contains("@"))
            {
                StatusColor = "#FFD32F2F";
                StatusMessage = "Wrong email format";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                StatusColor = "#FFD32F2F";
                StatusMessage = "Password can't be empty";
                return false;
            }
            return true;
        }

        public async Task RegisterAsync(string password, string repeatPassword)
        {
            if (string.IsNullOrWhiteSpace(Nickname)) { StatusMessage = "Nickname is required"; return; }
            if (password != repeatPassword) { StatusMessage = "Passwords do not match"; return; }
            if (!Validate(password)) return;

            StatusMessage = "Signing up...";
            try
            {
                var session = await SupabaseService.Client.Auth.SignUp(Email, password);
                if (session?.User != null)
                {
                    StatusMessage = "Signed up successfully! You can now sign in.";
                    await Task.Delay(1500);
                    ShowLogin();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Sign-up error: {ex.Message}";
            }
        }

        public async Task<bool> LoginAsync(string password)
        {
            if (!Validate(password))
            {
                return false;
            }

            StatusColor = "#FF8B9AA2"; 
            StatusMessage = "Signing in...";

            try
            {
                var session = await SupabaseService.Client.Auth.SignIn(Email, password);
                if (session?.User != null)
                {
                    StatusColor = "#FF328A5D";
                    StatusMessage = $"Hello {session.User.Email}";
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                StatusColor = "#FFD32F2F"; 

                if (ex.Message.Contains("invalid_credentials"))
                {
                    StatusMessage = "Invalid email or password.";
                }
                else
                {
                    StatusMessage = "An error occurred. Please try again.";
                }
                return false;
            }
        }
        public async Task ResetPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                StatusMessage = "Please enter a valid email address to reset your password.";
            }

            StatusMessage = "Sending reset code...";

            try
            {
                await SupabaseService.Client.Auth.ResetPasswordForEmail(Email);
                StatusMessage = "Code sent! Check your inbox.";

                IsForgotEmailStep = false;
                IsForgotCodeStep = true;
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public async Task VerifyCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(ResetToken) || ResetToken.Length < 6) { StatusMessage = "Enter the valid code."; return; }

            StatusMessage = "Verifying code...";
            try
            {
                var session = await SupabaseService.Client.Auth.VerifyOTP(Email, ResetToken, Supabase.Gotrue.Constants.EmailOtpType.Recovery);

                if (session?.User != null)
                {
                    StatusMessage = "Code verified!";
                    IsForgotCodeStep = false;
                    IsForgotNewPasswordStep = true;
                }
            }
            catch (System.Exception)
            {
                StatusMessage = "Invalid code. Please try again.";
            }
        }
        public async Task UpdatePasswordAsync(string newPassword)
        {
            StatusMessage = "Updating password...";
            try
            {
                var attrs = new Supabase.Gotrue.UserAttributes { Password = newPassword };
                await SupabaseService.Client.Auth.Update(attrs);

                StatusMessage = "Password updated successfully!";
                await Task.Delay(1500);
                ShowLogin();
            }
            catch (System.Exception)
            {
                StatusMessage = "Error occurred while updating password.";
            }
        }
    }
}