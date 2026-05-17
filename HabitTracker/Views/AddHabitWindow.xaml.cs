using System.Windows;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class AddHabitWindow : Window
{
    public AddHabitViewModel ViewModel { get; }

    public AddHabitWindow(DashboardViewModel parentViewModel = null)
    {
        InitializeComponent();
        
        ViewModel = new AddHabitViewModel(parentViewModel);
        DataContext = ViewModel;
        
        // Set owner if parent window is available
        if (parentViewModel != null)
        {
            Owner = Window.GetWindow(Application.Current.MainWindow);
        }

        Loaded += (s, e) =>
        {
            // Bind radio button events for frequency change
            FreqDaily.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqWeekly.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqMonthly.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Collapsed;
            FreqSpecific.Checked += (_, __) => SchedulePanel.Visibility = Visibility.Visible;
        };
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
        if (ViewModel != null)
        {
            var success = await ViewModel.SaveHabitAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
