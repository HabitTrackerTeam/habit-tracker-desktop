using System.Windows;
using Microsoft.Win32;
using Supabase.Gotrue;

namespace HabitTracker.Views
{
    public partial class EditProfileWindow : Window
    {
        public string NewFullName { get; private set; } = string.Empty;

        public EditProfileWindow(string currentName)
        {
            InitializeComponent();
            FullNameInput.Text = currentName;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameInput.Text))
            {
                ErrorText.Text = "Full name cannot be empty.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            NewFullName = FullNameInput.Text;
            DialogResult = true;
            Close();
        }

        private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select an Avatar Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // To be implemented: upload image to storage and update UI
                MessageBox.Show("Photo selection handled. Implementation pending.", "Change Photo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
