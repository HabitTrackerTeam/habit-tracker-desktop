using System.Windows;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private DashboardViewModel _dashboardVM;
    public DashboardView()
    {
        InitializeComponent();

        _dashboardVM=new DashboardViewModel();

        MainContentArea.DataContext = _dashboardVM;

        this.Loaded+=DashboardView_Loaded;
    }


    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        ViewModel.ShowAccountSelection();
    }

    internal void ThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.HandleThemeToggle(sender, e);
    }

    public void UpdateThemeToggleVisuals(bool isDark)
    {
        string icon = isDark ? "🌙" : "☀️";
        string label = isDark ? "Dark Mode" : "Light Mode";

        if (DashboardThemeIcon != null)
        {
            DashboardThemeIcon.Text = icon;
        }

        if (DashboardThemeLabel != null)
        {
            DashboardThemeLabel.Text = label;
        }
    }

    public void SyncThemeToggle(bool isDark)
    {
        if (DashboardThemeToggle != null && DashboardThemeToggle.IsChecked != isDark)
        {
            DashboardThemeToggle.IsChecked = isDark;
        }
    }

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.LoadFormDataAsync();
        await _dashboardVM.LoadHabitsAsync();
    }

    private async void AddHabit_Click(object sender, RoutedEventArgs e)
    {
        await _dashboardVM.CreateHabitAsync();
    }

    private void SidebarNewHabit_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.IsAddFormVisible = !_dashboardVM.IsAddFormVisible;
        }
    }

    private async void SaveHabit_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            await _dashboardVM.CreateHabitAsync();
        }
    }
}
