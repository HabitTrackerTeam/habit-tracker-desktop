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
            string actualPassword = LoginPassword.Visibility == Visibility.Visible ? LoginPassword.Password : LoginPasswordVisible.Text;
            bool success = await ViewModel.LoginAsync(actualPassword);

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
        string password = RegisterPassword.Visibility == Visibility.Visible ? RegisterPassword.Password : RegisterPasswordVisible.Text;
        string repeatPassword = RegisterRepeatPassword.Visibility == Visibility.Visible ? RegisterRepeatPassword.Password : RegisterRepeatPasswordVisible.Text;
        if (password != repeatPassword)
        {
            ViewModel.StatusColor = "#FFD32F2F";
            ViewModel.StatusMessage = "Passwords do not match!";
            return;
        }
        bool success = await ViewModel.RegisterAsync(password);

        if (success)
        {
            RegisterPassword.Password = string.Empty;
            RegisterPasswordVisible.Text = string.Empty;
            RegisterRepeatPassword.Password = string.Empty;
            RegisterRepeatPasswordVisible.Text = string.Empty;
            ViewModel.ResetPasswordChecklist();

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
        string newPassword = ForgotNewPassword.Visibility == Visibility.Visible ? ForgotNewPassword.Password : ForgotNewPasswordVisible.Text;
        string repeatPassword = ForgotRepeatPassword.Visibility == Visibility.Visible ? ForgotRepeatPassword.Password : ForgotRepeatPasswordVisible.Text;
        if (newPassword != repeatPassword)
        {
            ViewModel.StatusMessage = "Passwords do not match!";
            return;
        }

        bool updated = await ViewModel.UpdatePasswordAsync(newPassword);

        if (updated)
        {
            ForgotNewPassword.Password = string.Empty;
            ForgotNewPasswordVisible.Text = string.Empty;
            ForgotRepeatPassword.Password = string.Empty;
            ForgotRepeatPasswordVisible.Text = string.Empty;
            ViewModel.ResetPasswordChecklist();
            ResetFormVisualState(LoginFormPanel);
        }
    }

    private void NavToRegister_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        RegisterPassword.Password = string.Empty;
        RegisterPasswordVisible.Text = string.Empty;
        RegisterRepeatPassword.Password = string.Empty;
        RegisterRepeatPasswordVisible.Text = string.Empty;
        ViewModel.ResetPasswordChecklist();

        SwitchAuthFormWithAnimation(() => ViewModel.ShowRegister(), RegisterFormPanel);
    }

    private void NavToLogin_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = string.Empty;
        LoginPassword.Password = string.Empty;
        ViewModel.ResetPasswordChecklist();

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
        ViewModel.ResetPasswordChecklist();
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

    private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var pb = (PasswordBox)sender;
        ViewModel.IsPasswordChecklistVisible = pb.Password.Length > 0;
        ViewModel.ValidatePassword(pb.Password);
    }

    private void ToggleRegisterPassword_Click(object sender, RoutedEventArgs e)
    {
        if (RegisterPassword.Visibility == Visibility.Visible)
        {
            RegisterPasswordVisible.Text = RegisterPassword.Password;
            RegisterPassword.Visibility = Visibility.Collapsed;
            RegisterPasswordVisible.Visibility = Visibility.Visible;
        }
        else
        {
            RegisterPassword.Password = RegisterPasswordVisible.Text;
            RegisterPasswordVisible.Visibility = Visibility.Collapsed;
            RegisterPassword.Visibility = Visibility.Visible;
        }
    }

    private void ToggleRegisterRepeatPassword_Click(object sender, RoutedEventArgs e)
    {
        if (RegisterRepeatPassword.Visibility == Visibility.Visible)
        {
            RegisterRepeatPasswordVisible.Text = RegisterRepeatPassword.Password;
            RegisterRepeatPassword.Visibility = Visibility.Collapsed;
            RegisterRepeatPasswordVisible.Visibility = Visibility.Visible;
        }
        else
        {
            RegisterRepeatPassword.Password = RegisterRepeatPasswordVisible.Text;
            RegisterRepeatPasswordVisible.Visibility = Visibility.Collapsed;
            RegisterRepeatPassword.Visibility = Visibility.Visible;
        }
    }

    private void ToggleForgotNewPassword_Click(object sender, RoutedEventArgs e)
    {
        if (ForgotNewPassword.Visibility == Visibility.Visible)
        {
            ForgotNewPasswordVisible.Text = ForgotNewPassword.Password;
            ForgotNewPassword.Visibility = Visibility.Collapsed;
            ForgotNewPasswordVisible.Visibility = Visibility.Visible;
        }
        else
        {
            ForgotNewPassword.Password = ForgotNewPasswordVisible.Text;
            ForgotNewPasswordVisible.Visibility = Visibility.Collapsed;
            ForgotNewPassword.Visibility = Visibility.Visible;
        }
    }

    private void ToggleForgotRepeatPassword_Click(object sender, RoutedEventArgs e)
    {
        if (ForgotRepeatPassword.Visibility == Visibility.Visible)
        {
            ForgotRepeatPasswordVisible.Text = ForgotRepeatPassword.Password;
            ForgotRepeatPassword.Visibility = Visibility.Collapsed;
            ForgotRepeatPasswordVisible.Visibility = Visibility.Visible;
        }
        else
        {
            ForgotRepeatPassword.Password = ForgotRepeatPasswordVisible.Text;
            ForgotRepeatPasswordVisible.Visibility = Visibility.Collapsed;
            ForgotRepeatPassword.Visibility = Visibility.Visible;
        }
    }

    private void RegisterPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.IsPasswordChecklistVisible = RegisterPasswordVisible.Text.Length > 0;
        ViewModel.ValidatePassword(RegisterPasswordVisible.Text);
    }

    private void ForgotNewPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.IsPasswordChecklistVisible = ForgotNewPasswordVisible.Text.Length > 0;
        ViewModel.ValidatePassword(ForgotNewPasswordVisible.Text);
    }
}
