using System.Collections.Generic;
using System.Windows;
using HabitTracker.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace HabitTracker.Views;

public partial class AddHabitWindow : Window
{
    public AddHabitViewModel ViewModel { get; }
    private UIElement? _ownerContentElement;
    private Effect? _ownerPreviousEffect;

    public AddHabitWindow(DashboardViewModel? parentViewModel = null)
    {
        InitializeComponent();

        ViewModel = new AddHabitViewModel(parentViewModel);
        DataContext = ViewModel;

        Loaded += (s, e) =>
        {
            ApplyOwnerBlur();
            FreqDaily.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqWeekly.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqMonthly.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqSpecific.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Visible;
        };

        Closed += (_, __) => RestoreOwnerBlur();
    }

    private void TypeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            ResetButtonGroup(new[] { TypeNumericBtn, TypeCheckboxBtn, TypeTimerBtn }, btn);
        }
    }

    private void PriorityButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            ResetButtonGroup(new[] { PriorityLowBtn, PriorityMediumBtn, PriorityHighBtn }, btn);
        }
    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            var bgBrush = btn.Background as System.Windows.Media.SolidColorBrush;
            bool isSelected = bgBrush == null || bgBrush.Color.R == 255;

            System.Windows.Media.Color green = System.Windows.Media.Color.FromArgb(255, 50, 138, 93);
            System.Windows.Media.Color gray = System.Windows.Media.Color.FromArgb(255, 221, 226, 229);
            System.Windows.Media.Color darkGray = System.Windows.Media.Color.FromArgb(255, 78, 96, 108);

            if (isSelected)
            {
                btn.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 230, 248, 240)
                );
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(green);
                btn.BorderThickness = new System.Windows.Thickness(2);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(green);
            }
            else
            {
                btn.Background = System.Windows.Media.Brushes.White;
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(gray);
                btn.BorderThickness = new System.Windows.Thickness(1);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(darkGray);
            }
        }
    }

    private void NewCategory_Click(object sender, RoutedEventArgs e)
    {
        var createCategoryWindow = new CreateCategoryWindow();
        createCategoryWindow.Owner = this;
        if (createCategoryWindow.ShowDialog() == true)
        {
            string categoryName = createCategoryWindow.GetCategoryName();
            MessageBox.Show(
                $"Category '{categoryName}' will be created!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }

    private void ResetButtonGroup(Button[] buttons, Button selectedBtn)
    {
        System.Windows.Media.Color green = System.Windows.Media.Color.FromArgb(255, 50, 138, 93);
        System.Windows.Media.Color gray = System.Windows.Media.Color.FromArgb(255, 221, 226, 229);
        System.Windows.Media.Color darkGray = System.Windows.Media.Color.FromArgb(255, 78, 96, 108);

        foreach (var btn in buttons)
        {
            if (btn == selectedBtn)
            {
                btn.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 230, 248, 240)
                );
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(green);
                btn.BorderThickness = new System.Windows.Thickness(2);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(green);
            }
            else
            {
                btn.Background = System.Windows.Media.Brushes.White;
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(gray);
                btn.BorderThickness = new System.Windows.Thickness(1);
                btn.Foreground = new System.Windows.Media.SolidColorBrush(darkGray);
            }
        }
    }

    private void ApplyOwnerBlur()
    {
        if (Owner?.Content is not UIElement ownerContent)
        {
            return;
        }

        _ownerContentElement = ownerContent;
        _ownerPreviousEffect = ownerContent.Effect;
        ownerContent.Effect = new BlurEffect { Radius = 8 };
    }

    private void RestoreOwnerBlur()
    {
        if (_ownerContentElement == null)
        {
            return;
        }

        _ownerContentElement.Effect = _ownerPreviousEffect;
        _ownerContentElement = null;
        _ownerPreviousEffect = null;
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

    private async void PlantButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await ViewModel.SaveHabitAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Wystąpił błąd podczas zapisywania nawyku: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
