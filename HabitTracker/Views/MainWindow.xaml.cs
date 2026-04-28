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

    public void HandleThemeToggle(object sender, RoutedEventArgs e)
    {
        var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
        bool isDark = toggle?.IsChecked == true;

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

        AuthViewControl.UpdateThemeToggleVisuals(isDark);
        DashboardViewControl.UpdateThemeToggleVisuals(isDark);
    }
}