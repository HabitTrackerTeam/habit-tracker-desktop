using System.Windows;
using System.Windows.Media.Imaging;
using HabitTracker.ViewModels;
using System.Windows.Media;
using HabitTracker.Models;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HabitTracker.Views;

public partial class AuthView : System.Windows.Controls.UserControl
{
    private LoginViewModel ViewModel => (LoginViewModel)DataContext;
    private bool _isFormTransitionRunning;

    public AuthView()
    {
        InitializeComponent();

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
        if (ViewModel.IsLoading)
        {
            return;
        }

        ViewModel.IsLoading = true;

        try
        {
            string actualPassowrd = LoginPassword.Visibility == Visibility.Visible ? LoginPassword.Password : LoginPasswordVisible.Text;
            bool success = await ViewModel.LoginAsync(LoginPassword.Password);

            if (!success)
            {
                LoginPassword.Password = string.Empty;
                LoginPasswordVisible.Text = string.Empty;
            }
            else
            {
                ViewModel.ShowDashboard();
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.WindowState = WindowState.Maximized;
                }
            }
        }
        finally
        {
            ViewModel.IsLoading = false;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        string password = RegisterPassword.Password;
        if (password != RegisterRepeatPassword.Password)
        {
            ViewModel.StatusColor = "#FFD32F2F";
            ViewModel.StatusMessage = "Passwords do not match!";
            return;
        }
        bool success = await ViewModel.RegisterAsync(password);

        if (success)
        {
            RegisterPassword.Password = string.Empty;
            RegisterRepeatPassword.Password = string.Empty;

            AvatarImageBrush.ImageSource = null;
            AvatarBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFDDE2E5");

            await Task.Delay(1500);

            SwitchAuthFormWithAnimation(() => ViewModel.ShowLogin(), LoginFormPanel);
        }
    }

    private async void SendResetLink_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResetPasswordAsync();
    }

    private async void VerifyOnlyCode_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.VerifyCodeAsync();
    }

    private async void SaveNewPassword_Click(object sender, RoutedEventArgs e)
    {
        if (ForgotNewPassword.Password != ForgotRepeatPassword.Password)
        {
            ViewModel.StatusMessage = "Passwords do not match!";
            return;
        }

        if (ForgotNewPassword.Password.Length < 6)
        {
            ViewModel.StatusMessage = "Password must be at least 6 characters.";
            return;
        }
        bool updated = await ViewModel.UpdatePasswordAsync(ForgotNewPassword.Password);

        if (updated)
        {
            ForgotNewPassword.Password = string.Empty;
            ForgotRepeatPassword.Password = string.Empty;
            ResetFormVisualState(LoginFormPanel);
        }
    }

    private void NavToRegister_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        RegisterPassword.Password = string.Empty;

        SwitchAuthFormWithAnimation(() => ViewModel.ShowRegister(), RegisterFormPanel);
    }

    private void NavToLogin_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        LoginPassword.Password = string.Empty;

        if (RegisterFormPanel.Visibility == Visibility.Visible || ForgotFormPanel.Visibility == Visibility.Visible)
        {
            SwitchAuthFormWithAnimation(() => ViewModel.ShowLogin(), LoginFormPanel);
            return;
        }

        ViewModel.ShowLogin();
        ResetFormVisualState(LoginFormPanel);
    }

    private void NavToForgot_Click(object sender, RoutedEventArgs e)
    {
        SwitchAuthFormWithAnimation(() => ViewModel.ShowForgot(), ForgotFormPanel);
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

    private void ChooseAvatar_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

        openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

        //jesli uzytkownik wybral plik i zatwierdzil ok
        if (openFileDialog.ShowDialog() == true)
        {
            string selectedFilePath = openFileDialog.FileName;

            ViewModel.AvatarPath = selectedFilePath;

            // wyswietlenie zdjecia w okraglej ramce
            AvatarImageBrush.ImageSource = new BitmapImage(new Uri(selectedFilePath));
            //sukces-zmiana koloru obramowania
            AvatarBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF328A5D");
        }
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

        if (LoginThemeIcon != null)
        {
            LoginThemeIcon.Text = icon;
        }

        if (LoginThemeLabel != null)
        {
            LoginThemeLabel.Text = label;
        }
    }

    public void SyncThemeToggle(bool isDark)
    {
        if (LoginThemeToggle != null && LoginThemeToggle.IsChecked != isDark)
        {
            LoginThemeToggle.IsChecked = isDark;
        }
    }

    private void SavedAccount_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;

        var account = button?.DataContext as SavedAccount;

        if (account != null)
        {

            ViewModel.Email = account.Email;
            ViewModel.ShowLogin();

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
