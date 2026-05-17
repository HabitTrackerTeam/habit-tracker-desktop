using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class CreateCategoryWindow : Window
{
    public string SelectedIcon { get; private set; }
    public string SelectedColor { get; private set; }

    public CreateCategoryWindow()
    {
        InitializeComponent();
        SelectedIcon = "✓";
        SelectedColor = "#328A5D";
        UpdateIconSelection(IconBtn1);
        UpdateColorSelection(ColorBtn1);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectedIcon = btn.Content?.ToString();
            UpdateIconSelection(btn);
        }
    }

    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectedColor = btn.Background?.ToString();
            UpdateColorSelection(btn);
        }
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        string categoryName = CategoryNameBox.Text?.Trim();
        
        if (string.IsNullOrEmpty(categoryName))
        {
            MessageBox.Show("Please enter a category name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void UpdateIconSelection(Button selectedBtn)
    {
        var children = (selectedBtn.Parent as Panel)?.Children;
        if (children == null) return;

        var selectedBackground = TryFindResource("BrushPrimary") as System.Windows.Media.Brush
            ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 138, 93));

        foreach (var child in children)
        {
            if (child is Button btn)
            {
                btn.Background = btn == selectedBtn
                    ? selectedBackground
                    : System.Windows.Media.Brushes.White;
                btn.Foreground = btn == selectedBtn
                    ? System.Windows.Media.Brushes.White
                    : System.Windows.Media.Brushes.Black;
                btn.BorderBrush = btn == selectedBtn
                    ? System.Windows.Media.Brushes.Transparent
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 221, 226, 229));
                btn.BorderThickness = btn == selectedBtn ? new Thickness(0) : new Thickness(1);
            }
        }
    }

    private void UpdateColorSelection(Button selectedBtn)
    {
        var parentPanel = selectedBtn.Parent as StackPanel;
        if (parentPanel == null) return;

        foreach (var child in parentPanel.Children)
        {
            if (child is Button btn && btn.Width == 40)
            {
                if (btn == selectedBtn)
                {
                    btn.BorderThickness = new Thickness(3);
                    btn.BorderBrush = System.Windows.Media.Brushes.Black;
                }
                else
                {
                    btn.BorderThickness = new Thickness(0);
                    btn.BorderBrush = null;
                }
            }
        }
    }

    public string GetCategoryName() => CategoryNameBox.Text?.Trim();
}
