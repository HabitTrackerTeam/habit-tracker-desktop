using System.Windows;
using System.Windows.Media.Imaging;
using HabitTracker.ViewModels;
using System.Windows.Media;
using HabitTracker.Models;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HabitTracker;


public partial class MainWindow : Window
{
    private LoginViewModel _viewModel;
    private bool _isFormTransitionRunning;

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

        Loaded += (_, _) =>
        {
            ResetFormVisualState(LoginFormPanel);
            ResetFormVisualState(RegisterFormPanel);
            ResetFormVisualState(ForgotFormPanel);
            UpdateThemeToggleVisuals(false);
        };
    }


    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsLoading)
        {
            return;
        }

        _viewModel.IsLoading = true;

        try
        {
            string actualPassowrd = LoginPassword.Visibility == Visibility.Visible ? LoginPassword.Password : LoginPasswordVisible.Text;
            bool success = await _viewModel.LoginAsync(LoginPassword.Password);

            if (!success)
            {
                LoginPassword.Password = string.Empty;
                LoginPasswordVisible.Text = string.Empty;
            }
            else
            {
                _viewModel.ShowDashboard();
                this.WindowState = WindowState.Maximized; //skalujemy okno, po zalogowaniu
            }
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Email = string.Empty;
        LoginPassword.Password = string.Empty;
        _viewModel.ShowAccountSelection();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        string password = RegisterPassword.Password;
        if (password != RegisterRepeatPassword.Password)
        {
            _viewModel.StatusColor = "#FFD32F2F";
            _viewModel.StatusMessage = "Passwords do not match!";
            return;
        }
        bool success = await _viewModel.RegisterAsync(password);

        if (success)
        {
            RegisterPassword.Password = string.Empty;
            RegisterRepeatPassword.Password = string.Empty;

            AvatarImageBrush.ImageSource = null;
            AvatarBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFDDE2E5");

            await Task.Delay(1500);

            SwitchAuthFormWithAnimation(() => _viewModel.ShowLogin(), LoginFormPanel);
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
        bool updated = await _viewModel.UpdatePasswordAsync(ForgotNewPassword.Password);

        if (updated)
        {
            ForgotNewPassword.Password = string.Empty;
            ForgotRepeatPassword.Password = string.Empty;
            ResetFormVisualState(LoginFormPanel);
        }
    }
    private void NavToRegister_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Email = string.Empty;
        RegisterPassword.Password = string.Empty;

        SwitchAuthFormWithAnimation(() => _viewModel.ShowRegister(), RegisterFormPanel);
    }

    private void NavToLogin_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Email = string.Empty;
        LoginPassword.Password = string.Empty;

        if (RegisterFormPanel.Visibility == Visibility.Visible || ForgotFormPanel.Visibility == Visibility.Visible)
        {
            SwitchAuthFormWithAnimation(() => _viewModel.ShowLogin(), LoginFormPanel);
            return;
        }

        _viewModel.ShowLogin();
        ResetFormVisualState(LoginFormPanel);
    }

    private void NavToForgot_Click(object sender, RoutedEventArgs e)
    {
        SwitchAuthFormWithAnimation(() => _viewModel.ShowForgot(), ForgotFormPanel);
    }

    private static TranslateTransform EnsureTranslateTransform(UIElement element)
    {
        if (element.RenderTransform is TranslateTransform translate)
        {
            return translate;
        }

        var newTransform = new TranslateTransform();
        element.RenderTransform = newTransform;
        return newTransform;
    }

    private static void ResetFormVisualState(UIElement form)
    {
        var transform = EnsureTranslateTransform(form);
        transform.BeginAnimation(TranslateTransform.XProperty, null);
        form.BeginAnimation(UIElement.OpacityProperty, null);
        transform.X = 0;
        form.Opacity = 1;
    }

    private void AnimateFormOut(UIElement form, Action onCompleted)
    {
        var transform = EnsureTranslateTransform(form);

        var slideOut = new DoubleAnimation
        {
            To = -40, //przesuniecie o 40 pikseli w lewo
            Duration = TimeSpan.FromSeconds(0.25), // wykonuje sie w 1/4 sekundy, dla lepszego efektu
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } //wolne rozpoczecie, szybsze na koncu
        };

        var fadeOut = new DoubleAnimation
        {
            To = 0, //zmiana widocznosci na 0
            Duration = TimeSpan.FromSeconds(0.25),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fadeOut.Completed += (_, _) => onCompleted(); //notyfikacja gdy skonczy sie znikanie

        transform.BeginAnimation(TranslateTransform.XProperty, slideOut);
        form.BeginAnimation(UIElement.OpacityProperty, fadeOut);
    }

    private void AnimateFormIn(UIElement form)
    {
        var transform = EnsureTranslateTransform(form);
        transform.X = 40;
        form.Opacity = 0; //formularz niewidoczny na starcie

        var slideIn = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(0.28),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } //szybszy poczatek, wolniejszy koniec animacji
        };

        var fadeIn = new DoubleAnimation
        {
            To = 1, //widocznosc
            Duration = TimeSpan.FromSeconds(0.28),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
        form.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }

    private StackPanel? GetCurrentAnimatedForm()
    {
        if (LoginFormPanel.Visibility == Visibility.Visible)
        {
            return LoginFormPanel;
        }

        if (RegisterFormPanel.Visibility == Visibility.Visible)
        {
            return RegisterFormPanel;
        }

        if (ForgotFormPanel.Visibility == Visibility.Visible)
        {
            return ForgotFormPanel;
        }

        return null;
    }

    private void SwitchAuthFormWithAnimation(Action switchViewAction, StackPanel targetForm)
    {
        if (_isFormTransitionRunning)
        {
            return;
        }

        var currentForm = GetCurrentAnimatedForm();

        if (currentForm == null || currentForm == targetForm)
        {
            switchViewAction();
            ResetFormVisualState(targetForm);
            return;
        }

        _isFormTransitionRunning = true; //blokada ekranu na czas animacji

        AnimateFormOut(currentForm, () =>
        {
            switchViewAction();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ResetFormVisualState(targetForm);
                AnimateFormIn(targetForm);
                _isFormTransitionRunning = false;
            }), DispatcherPriority.Loaded);
        });
    }

    private void FloatingPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetHasText(passwordBox, !string.IsNullOrEmpty(passwordBox.Password));
        }
    }

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

        if (LoginThemeToggle != null && LoginThemeToggle.IsChecked != isDark)
        {
            LoginThemeToggle.IsChecked = isDark;
        }

        if (DashboardThemeToggle != null && DashboardThemeToggle.IsChecked != isDark)
        {
            DashboardThemeToggle.IsChecked = isDark;
        }

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
            UpdateThemeToggleVisuals(true);
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
            UpdateThemeToggleVisuals(false);
        }
    }

    private void UpdateThemeToggleVisuals(bool isDark)
    {
        string icon = isDark ? "🌙" : "☀️";
        string label = isDark ? "Dark Mode" : "Light Mode";

        if (LoginThemeIcon != null)
        {
            LoginThemeIcon.Text = icon;
        }

        if (LoginThemeLabel != null)
        {
            LoginThemeLabel.Text = label;
        }

        if (DashboardThemeIcon != null)
        {
            DashboardThemeIcon.Text = icon;
        }

        if (DashboardThemeLabel != null)
        {
            DashboardThemeLabel.Text = label;
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

    private void ToggleLoginPassword_Click(object sender, RoutedEventArgs e)
    {
        if (LoginPassword.Visibility == Visibility.Visible)
        {
            LoginPasswordVisible.Text = LoginPassword.Password;
            LoginPassword.Visibility = Visibility.Collapsed;
            LoginPasswordVisible.Visibility = Visibility.Visible;
        }
        else
        {
            LoginPassword.Password = LoginPasswordVisible.Text;
            LoginPasswordVisible.Visibility = Visibility.Collapsed;
            LoginPassword.Visibility = Visibility.Visible;

        }
    }

}