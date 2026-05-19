using System.Windows;
using System.Windows.Input;

namespace HabitTracker.Views
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string title, string message)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        public static void Show(string message, string title = "Notification")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var msgBox = new CustomMessageBox(title, message);
                msgBox.ShowDialog();
            });
        }
    }
}
