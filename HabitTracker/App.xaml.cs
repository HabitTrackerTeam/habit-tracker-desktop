using System.Configuration;
using System.Data;
using System.Windows;
using HabitTracker.Services;

namespace HabitTracker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await SupabaseService.InitializeAsync();
        }
    }

