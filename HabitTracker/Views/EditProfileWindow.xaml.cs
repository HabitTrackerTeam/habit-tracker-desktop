using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace HabitTracker.Views
{
    public partial class EditProfileWindow : Window
    {
        public string NewFullName { get; private set; } = string.Empty;
        public string? SelectedPhotoPath { get; private set; }

        public EditProfileWindow(string currentName, string? currentAvatarUrl = null)
        {
            InitializeComponent();
            FullNameInput.Text = currentName;

            // Set initial
            if (!string.IsNullOrEmpty(currentName))
            {
                AvatarInitial.Text = currentName[0].ToString().ToUpper();
            }

            // If there's an existing avatar URL, show it
            if (!string.IsNullOrEmpty(currentAvatarUrl))
            {
                try
                {
                    var bitmap = new BitmapImage(new System.Uri(currentAvatarUrl));
                    AvatarImage.ImageSource = bitmap;
                    AvatarImageEllipse.Visibility = Visibility.Visible;
                    AvatarInitial.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    // If loading fails, keep showing the initial
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
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

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select an Avatar Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPhotoPath = openFileDialog.FileName;

                // Show preview of the selected image
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new System.Uri(SelectedPhotoPath);
                    bitmap.DecodePixelWidth = 200;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    AvatarImage.ImageSource = bitmap;
                    AvatarImageEllipse.Visibility = Visibility.Visible;
                    AvatarInitial.Visibility = Visibility.Collapsed;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Could not load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SelectedPhotoPath = null;
                }
            }
        }
    }
}
