using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using HabitTracker.ViewModels;
using System.Windows.Media;
using HabitTracker.Models;
using System.Windows.Controls;

namespace HabitTracker;


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
            _viewModel.ShowDashboard();
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
        bool success = await _viewModel.RegisterAsync(RegisterPassword.Password);

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

    private void ThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
        bool isDark = toggle.IsChecked == true;

        if (isDark)
        {
            //Tryb ciemny
            var bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new System.Windows.Point(0, 0);
            bgBrush.EndPoint = new System.Windows.Point(1, 1);
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF121212"), 0.0));
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF1E1E1E"), 1.0));

            this.Resources["AppBgBrush"] = bgBrush;
            this.Resources["CardBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF2D2D30");
            this.Resources["TextMainBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFE0E0E0");
            this.Resources["TextMutedBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFA0A0A0");
            this.Resources["InputBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF1E1E1E");
            this.Resources["InputBorderBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF434346");
        }
        else
        {
            //Tryb jasny
            var bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new System.Windows.Point(0, 0);
            bgBrush.EndPoint = new System.Windows.Point(1, 1);
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF4F4E9"), 0.0));
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE4E8E5"), 1.0));

            this.Resources["AppBgBrush"] = bgBrush;
            this.Resources["CardBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("White");
            this.Resources["TextMainBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF4E606C");
            this.Resources["TextMutedBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF8B9AA2");
            this.Resources["InputBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF8F9FA");
            this.Resources["InputBorderBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFDDE2E5");
        }
    }

    private void SavedAccount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;

        var account = button?.DataContext as SavedAccount;

        if (account != null)
        {

            _viewModel.Email = account.Email;
            _viewModel.ShowLogin();

            LoginPassword.Focus();

            
        }
    }

    private void NavToAccountSelection_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowAccountSelection();

        LoginPassword.Password = string.Empty;
    }
}