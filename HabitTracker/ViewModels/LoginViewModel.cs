using System.ComponentModel;
using System.Runtime.CompilerServices;
using HabitTracker.Services;
namespace HabitTracker.ViewModels
{
    public class LoginViewModel : ViewModelBase // Listener for input values
    {
        private string _email = string.Empty;
        private string _statusMessage = string.Empty;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                StatusMessage = "Email can't be empty";
                return false;
            }
            if (!Email.Contains("@"))
            {
                StatusMessage = "Wrong email format";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                StatusMessage = "Password can't be empty";
                return false;
            }
            return true;
        }

        public async Task RegisterAsync(Supabase.Client client, string password)
        {
            if (!Validate(password)) return;

            StatusMessage = "Signing up...";
            try
            {
                var session = await client.Auth.SignUp(Email, password);
                if (session?.User != null)
                {
                    StatusMessage = "Signed up successfully! You can now sign in.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Sign-up error: {ex.Message}";
            }
        }

        public async Task LoginAsync(string password)
        {
            if (!Validate(password)) return;

            StatusMessage = "Signing in...";
            try
            {

                var session = await SupabaseService.Client.Auth.SignIn(Email, password);

                if (session?.User != null)
                {
                    StatusMessage = $"Hello {session.User.Email}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error {ex.Message}";
            }
        }
    }
}