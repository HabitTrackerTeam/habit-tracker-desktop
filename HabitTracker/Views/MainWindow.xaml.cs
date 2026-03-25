using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using HabitTracker.ViewModels;
using System.Windows.Media;

namespace HabitTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private LoginViewModel _viewModel;
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        this.DataContext = _viewModel;
    }


    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        bool success = await _viewModel.LoginAsync(LoginPassword.Password);

        if (!success)
        {
            LoginPassword.Password = string.Empty;
        }
        else
        {
            //tutaj planowo bedzie przejscie do dashboardu
            LoginPassword.Password = string.Empty;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (RegisterPassword.Password != RegisterRepeatPassword.Password)
        {
            _viewModel.StatusColor = "#FFD32F2F";
            _viewModel.StatusMessage = "Passwords do not match!";
            return;
        }
        bool success = await _viewModel.RegisterAsync(RegisterPassword.Password, RegisterRepeatPassword.Password);

        if (success)
        {
            RegisterPassword.Password = string.Empty;
            RegisterRepeatPassword.Password = string.Empty;

            AvatarImageBrush.ImageSource = null;
            AvatarBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFDDE2E5");

            await Task.Delay(1500);

            _viewModel.ShowLogin();
        }
    }
    private async void SendResetLink_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ResetPasswordAsync();
    }

    private async void VerifyOnlyCode_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.VerifyCodeAsync();
    }

    private async void SaveNewPassword_Click(object sender, RoutedEventArgs e)
    {
        if (ForgotNewPassword.Password != ForgotRepeatPassword.Password)
        {
            _viewModel.StatusMessage = "Passwords do not match!";
            return;
        }

        if (ForgotNewPassword.Password.Length < 6)
        {
            _viewModel.StatusMessage = "Password must be at least 6 characters.";
            return;
        }
        await _viewModel.UpdatePasswordAsync(ForgotNewPassword.Password);
    }
    private void NavToRegister_Click(object sender, RoutedEventArgs e) => _viewModel.ShowRegister();
    private void NavToLogin_Click(object sender, RoutedEventArgs e) => _viewModel.ShowLogin();
    private void NavToForgot_Click(object sender, RoutedEventArgs e) => _viewModel.ShowForgot();

    private void ChooseAvatar_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

        openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

        //jesli uzytkownik wybral plik i zatwierdzil ok
        if (openFileDialog.ShowDialog() == true)
        {
            string selectedFilePath = openFileDialog.FileName;

            _viewModel.AvatarPath = selectedFilePath;

            // wyswietlenie zdjecia w okraglej ramce
            AvatarImageBrush.ImageSource = new BitmapImage(new Uri(selectedFilePath));
            //sukces-zmiana koloru obramowania
            AvatarBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF328A5D");
        }
    }
}