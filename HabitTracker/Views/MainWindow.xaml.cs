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
using Supabase;

namespace HabitTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Client _supabaseClient;

    public MainWindow()
    {
        InitializeComponent();
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
        StatusMessage.Text = "Logowanie...";
        StatusMessage.Foreground = System.Windows.Media.Brushes.Black;

        try
        {
            var email = EmailInput.Text;
            var password = PasswordInput.Password;

            var session = await _supabaseClient.Auth.SignIn(email, password);

            if (session?.User != null)
            {
                StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
                StatusMessage.Text = $"Zalogowano pomyślnie! ID: {session.User.Id}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
            StatusMessage.Text = $"Błąd logowania: {ex.Message}";
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        StatusMessage.Text = "Rejestracja...";
        StatusMessage.Foreground = System.Windows.Media.Brushes.Black;

        try
        {
            var email = EmailInput.Text;
            var password = PasswordInput.Password;

            var session = await _supabaseClient.Auth.SignUp(email, password);

            if (session?.User != null)
            {
                StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
                StatusMessage.Text = $"Zarejestrowano pomyślnie! Możesz się teraz zalogować.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
            StatusMessage.Text = $"Błąd rejestracji: {ex.Message}";
        }
    }

}