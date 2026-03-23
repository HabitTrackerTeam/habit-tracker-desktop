using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HabitTracker.ViewModels;
using Supabase;

namespace HabitTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Client _supabaseClient;
    private LoginViewModel _viewModel;
    public MainWindow()
    {
        InitializeComponent();
        _viewModel=new LoginViewModel();
        this.DataContext=_viewModel;
        InitializeSupabase();
    }

    private async void InitializeSupabase()
    {
        var url = "https://fkhmrfueypnrbkdvqiyn.supabase.co";
        var key = "sb_publishable_ckwd846nvtwV6oXPEuqq7w_QKFvhiQU";

        _supabaseClient = new Client(url, key);
        await _supabaseClient.InitializeAsync();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string password = PasswordInput.Password;

        if(_viewModel.Validate(password)){
            _viewModel.StatusMessage = "Signing in...";
        }

        try
        {

            var session = await _supabaseClient.Auth.SignIn(_viewModel.Email, password);

            if (session?.User != null)
            {
                _viewModel.StatusMessage = $"Hello {session.User.Email}";
            }
        }
        catch (Exception ex)
        {
            _viewModel.StatusMessage = $"Error {ex.Message}";
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        StatusMessage.Text = "Signing up...";
        StatusMessage.Foreground = System.Windows.Media.Brushes.Black;

        try
        {
            var email = EmailInput.Text;
            var password = PasswordInput.Password;

            var session = await _supabaseClient.Auth.SignUp(email, password);

            if (session?.User != null)
            {
                StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
                StatusMessage.Text = "Signed up successfully! You can now sign in.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
            StatusMessage.Text = $"Sign-up error: {ex.Message}";
        }
    }

}