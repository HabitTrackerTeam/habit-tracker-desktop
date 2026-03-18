using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.ViewModels
{
    // To jest bazowa klasa dla Twojej logiki widoku (ViewModel)
    // Na razie nic nie robi, ale przygotowuje grunt pod powiadomienia o zmianach danych
    public class TestViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "System gotowy do pracy...";

        public string StatusMessage
        {
            get => _statusMessage;
            set 
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Standardowy mechanizm WPF do odświeżania widoku
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Przykładowa metoda, którą będziesz mógł wywołać w przyszłości
        public void Initialize()
        {
            // Miejsce na przyszłą logikę startową
        }
    }
}