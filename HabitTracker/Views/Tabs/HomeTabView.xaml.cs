using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;

namespace HabitTracker.Views.Tabs
{
    public partial class HomeTabView : UserControl
    {
        private DashboardViewModel _dashboardVM => DataContext as DashboardViewModel;

        public HomeTabView()
        {
            InitializeComponent();
            this.Loaded += HomeTabView_Loaded;
        }

        private async void HomeTabView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
            {
                await _dashboardVM.LoadHabitsAsync();
                await _dashboardVM.LoadDailyNoteAsync();
            }
        }
    }
}
