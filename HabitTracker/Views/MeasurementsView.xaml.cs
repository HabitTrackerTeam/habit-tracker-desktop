using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;

namespace HabitTracker.Views
{
    public partial class MeasurementsView : UserControl
    {
        public MeasurementsView()
        {
            InitializeComponent();
        }

        private async void SaveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                await vm.SaveMeasurementAsync();
            }
        }
    }
}