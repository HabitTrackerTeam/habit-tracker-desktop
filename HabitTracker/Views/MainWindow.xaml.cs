using System.Windows;
using System.Windows.Media;
using HabitTracker.ViewModels;
using System.Windows.Controls;

namespace HabitTracker;


public partial class MainWindow : Window
{
    private LoginViewModel _viewModel;

    public static readonly DependencyProperty HasTextProperty = DependencyProperty.RegisterAttached(
        "HasText",
        typeof(bool),
        typeof(MainWindow),
        new PropertyMetadata(false));

    public static bool GetHasText(DependencyObject obj) => (bool)obj.GetValue(HasTextProperty);

    public static void SetHasText(DependencyObject obj, bool value) => obj.SetValue(HasTextProperty, value);

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        this.DataContext = _viewModel;

        // Obsługa PasswordChanged dla floating label w PasswordBox
        this.AddHandler(PasswordBox.PasswordChangedEvent, new RoutedEventHandler(FloatingPasswordBox_PasswordChanged));
    }

    private void FloatingPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is PasswordBox passwordBox)
        {
            SetHasText(passwordBox, !string.IsNullOrEmpty(passwordBox.Password));
        }
    }

    public async void HandleThemeToggle(object sender, RoutedEventArgs e)
    {
        var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
        bool isDark = toggle?.IsChecked == true;
        ApplyTheme(isDark);

        var currentUser = HabitTracker.Services.SupabaseService.Client.Auth.CurrentUser;
        if (currentUser != null)
        {
            var settings = await HabitTracker.Services.UserSettingsService.LoadSettingsAsync(currentUser.Id);
            if (settings != null)
            {
                settings.Theme = isDark ? "dark" : "light";
                await HabitTracker.Services.UserSettingsService.SaveSettingsAsync(settings);
            }
        }
    }

    public void ApplyTheme(bool isDark)
    {
        // Synchronizuj oba widoki
        AuthViewControl.SyncThemeToggle(isDark);
        DashboardViewControl.SyncThemeToggle(isDark);

        if (isDark)
        {
            //Tryb ciemny
            var bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new System.Windows.Point(0, 0);
            bgBrush.EndPoint = new System.Windows.Point(1, 1);
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF121212"), 0.0));
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF1E1E1E"), 1.0));

            Application.Current.Resources["AppBgBrush"] = bgBrush;
            Application.Current.Resources["CardBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF2D2D30");
            Application.Current.Resources["TextMainBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFE0E0E0");
            Application.Current.Resources["TextMutedBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFA0A0A0");
            Application.Current.Resources["InputBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF1E1E1E");
            Application.Current.Resources["InputBorderBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF434346");
        }
        else
        {
            //Tryb jasny
            Application.Current.Resources["AppBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#F8FAFC");
            Application.Current.Resources["CardBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("White");
            Application.Current.Resources["TextMainBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF4E606C");
            Application.Current.Resources["TextMutedBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF8B9AA2");
            Application.Current.Resources["InputBgBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF8F9FA");
            Application.Current.Resources["InputBorderBrush"] = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFDDE2E5");
        }

        AuthViewControl.UpdateThemeToggleVisuals(isDark);
        DashboardViewControl.UpdateThemeToggleVisuals(isDark);
    }
}