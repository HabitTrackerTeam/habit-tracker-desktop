using System.Windows;
using HabitTracker.ViewModels;


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
        await _viewModel.LoginAsync(LoginPassword.Password);
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.RegisterAsync(RegisterPassword.Password, RegisterRepeatPassword.Password);
    }

    private async void SendResetLink_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ResetPasswordAsync();
    }

    private void NavToRegister_Click(object sender, RoutedEventArgs e) => _viewModel.ShowRegister();
    private void NavToLogin_Click(object sender, RoutedEventArgs e) => _viewModel.ShowLogin();
    private void NavToForgot_Click(object sender, RoutedEventArgs e) => _viewModel.ShowForgot();
}