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
        UpdateSidebar(NavDashboard);
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

    private void SwitchToHabits_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToHabits();
            if (sender is System.Windows.Controls.Button btn && (btn == NavDashboard || btn == NavHabits))
            {
                UpdateSidebar(btn);
            }
            else
            {
                UpdateSidebar(NavDashboard);
            }
        }
    }

    private void SwitchToMeasurements_Click(object sender, RoutedEventArgs e)
    {
        if (_dashboardVM != null)
        {
            _dashboardVM.SwitchToMeasurements();
            UpdateSidebar(NavMeasurements);
        }
    }
    
    private void UpdateSidebar(System.Windows.Controls.Button activeBtn)
    {
        // Remove the active style and reset properties
        if (NavDashboard != null)
        {
            NavDashboard.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
            if(NavDashboard.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                if(sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                if(sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty); tb2.ClearValue(System.Windows.Controls.TextBlock.FontWeightProperty); }
            }
        }
        
        if (NavHabits != null)
        {
            NavHabits.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
            if(NavHabits.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                if(sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                if(sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty); tb2.ClearValue(System.Windows.Controls.TextBlock.FontWeightProperty); }
            }
        }
        
        if (NavMeasurements != null)
        {
            NavMeasurements.ClearValue(System.Windows.Controls.Button.BackgroundProperty);
            if(NavMeasurements.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                if(sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty);
                if(sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.ClearValue(System.Windows.Controls.TextBlock.ForegroundProperty); tb2.ClearValue(System.Windows.Controls.TextBlock.FontWeightProperty); }
            }
        }
        
        // Apply active style (Green)
        if(activeBtn != null)
        {
            activeBtn.SetResourceReference(System.Windows.Controls.Button.BackgroundProperty, "InputBgBrush");
            if(activeBtn.Content is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 2)
            {
                var greenBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#328A5D"));
                if(sp.Children[0] is System.Windows.Controls.TextBlock tb1) tb1.Foreground = greenBrush;
                if(sp.Children[1] is System.Windows.Controls.TextBlock tb2) { tb2.Foreground = greenBrush; tb2.FontWeight = FontWeights.Bold; }
            }
        }
    }
}
