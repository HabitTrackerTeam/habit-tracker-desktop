using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;

namespace HabitTracker.Views.Tabs
{
    public partial class MeasurementsTabView : UserControl
    {
        public MeasurementsTabView()
        {
            InitializeComponent();
        }

        private async void SaveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MeasurementsViewModel vm)
            {
                await vm.SaveMeasurementAsync();
                vm.IsModalOpen = false; // Close modal on save
            }
        }
    }
}