using System.Windows;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    public DashboardView()
    {
        InitializeComponent();
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
}
