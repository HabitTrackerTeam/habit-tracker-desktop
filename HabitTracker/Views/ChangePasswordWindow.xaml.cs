using System.Windows;

namespace HabitTracker.Views;

public partial class ChangePasswordWindow : Window
{
    public string OldPassword { get; private set; } = string.Empty;
    public string NewPassword { get; private set; } = string.Empty;

    public ChangePasswordWindow()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OldPasswordBox.Password))
        {
            ErrorText.Text = "Old password cannot be empty.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
        {
            ErrorText.Text = "New password cannot be empty.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (NewPasswordBox.Password != RepeatPasswordBox.Password)
        {
            ErrorText.Text = "Passwords do not match.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (NewPasswordBox.Password.Length < 6)
        {
            ErrorText.Text = "Password must be at least 6 characters.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        OldPassword = OldPasswordBox.Password;
        NewPassword = NewPasswordBox.Password;
        DialogResult = true;
        Close();
    }
}
