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
using HabitTracker.Services;
using HabitTracker.ViewModels;
using Supabase;

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
        _viewModel=new LoginViewModel();
        this.DataContext=_viewModel;
    }


    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoginAsync(PasswordInput.Password);
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.RegisterAsync(SupabaseService.Client, PasswordInput.Password);
    }

}