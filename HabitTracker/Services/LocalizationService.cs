using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.Services
{
    /// <summary>
    /// Simple localization service providing Polish and English translations.
    /// Singleton pattern — all views reference the same instance.
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _currentLanguage = "pl";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(null); // Notify ALL properties changed
                }
            }
        }

        private string Get(string pl, string en)
        {
            return _currentLanguage == "en" ? en : pl;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // === Sidebar ===
        public string Home => Get("Strona główna", "Home");
        public string Dashboard => Get("Panel główny", "Dashboard");
        public string HabitsManager => Get("Zarządzanie nawykami", "Habits Manager");
        public string Calendar => Get("Kalendarz", "Calendar");
        public string Statistics => Get("Statystyki", "Statistics");
        public string Measurements => Get("Pomiary", "Measurements");
        public string PdfReport => Get("Raport PDF", "PDF Report");
        public string Settings => Get("Ustawienia", "Settings");
        public string Logout => Get("Wyloguj", "Logout");

        // === Dashboard ===
        public string MyHabits => Get("Moje nawyki", "My Habits");
        public string MyNotes => Get("Moje notatki", "My Notes");
        public string DashboardWelcome => Get(
            "Twoje nawyki pojawią się tutaj.\nWybierz z listy lub stwórz własne w Zarządzaniu nawykami.",
            "Personal habits will appear here.\nSelect from the built-in list or create your own in the Habits Manager.");

        // === Habits Manager ===
        public string WhatPlanToAchieve => Get("Co planujesz osiągnąć?", "What are you planning to achieve?");
        public string AddNewHabit => Get("Dodaj nowy nawyk", "Add New Habit");
        public string FromList => Get("Z listy", "From List");
        public string Custom => Get("Własny", "Custom");
        public string ChooseHabitFromList => Get("Wybierz nawyk z listy", "Choose a habit from the list");
        public string AddSelected => Get("Dodaj wybrany", "Add Selected");
        public string HabitName => Get("Nazwa nawyku", "Habit Name");

        public string Type => Get("Typ", "Type");
        public string SaveHabit => Get("Zapisz nawyk", "Save Habit");

        // === Settings ===
        public string ManageAccount => Get("Zarządzaj swoim kontem, preferencjami i integracjami.", "Manage your account, preferences, and integrations.");
        public string EditProfile => Get("Edytuj profil", "Edit Profile");
        public string ChangePassword => Get("Zmień hasło", "Change Password");
        public string General => Get("Ogólne", "General");
        public string Language => Get("Język", "Language");
        public string StartOfWeek => Get("Początek tygodnia", "Start of Week");
        public string Appearance => Get("Wygląd", "Appearance");
        public string DarkMode => Get("Tryb ciemny", "Dark Mode");
        public string DarkModeDesc => Get("Przełącz między jasnym a ciemnym motywem.", "Switch between light and dark UI themes.");
        public string Notifications => Get("Powiadomienia", "Notifications");
        public string DailyReminders => Get("Codzienne przypomnienia", "Daily Reminders");
        public string DailyRemindersDesc => Get("Powiadomienia push, aby utrzymać serię.", "Push notifications to keep your streak alive.");
        public string AchievementBadges => Get("Odznaki osiągnięć", "Achievement Badges");
        public string AchievementBadgesDesc => Get("Gdy zdobędziesz kamień milowy lub pobierzesz rekord.", "When you hit milestones or break records.");
        public string SaveSettings => Get("Zapisz ustawienia", "Save Settings");

        // === Calendar / Statistics / PDF ===
        public string TrackHabitsOverTime => Get("Śledź nawyki w czasie", "Track your habits over time");
        public string ViewProgressAchievements => Get("Zobacz swoje postępy i osiągnięcia", "View your progress and achievements");
        public string GenerateDownloadReports => Get("Generuj i pobieraj raporty", "Generate and download your reports");
        public string CalendarContentPlaceholder => Get("Zawartość kalendarza zostanie dodana tutaj.", "Calendar content will be added here.");
        public string StatisticsContentPlaceholder => Get("Zawartość statystyk zostanie dodana tutaj.", "Statistics content will be added here.");
        public string PdfReportContentPlaceholder => Get("Zawartość raportu PDF zostanie dodana tutaj.", "PDF Report content will be added here.");

        // === Measurements ===
        public string BodyMeasurements => Get("Pomiary ciała", "Body Measurements");
        public string TrackPhysicalProgression => Get("Śledź swoje postępy fizyczne", "Track your physical progression");
        public string LogBodyMeasurements => Get("Dodaj pomiary", "Log Body Measurements");
        public string Weight => Get("WAGA", "WEIGHT");
        public string Height => Get("WZROST", "HEIGHT");
        public string BmiIndex => Get("WSKAŹNIK BMI", "BMI INDEX");
        public string MeasurementsTrends => Get("Trendy pomiarów", "Measurements Trends");
        public string BodyMetrics => Get("METRYKI CIAŁA", "BODY METRICS");
        public string CurrentSelection => Get("OBECNY WYBÓR", "CURRENT SELECTION");
        public string ThisMonth => Get("w tym miesiącu", "this month");
        public string NewMeasurementSession => Get("Nowa sesja pomiarowa", "New Measurement Session");
        public string Date => Get("Data: ", "Date: ");
        public string WeightKg => Get("Waga (kg)", "Weight (kg)");
        public string HeightCm => Get("Wzrost (cm)", "Height (cm)");
        public string BodyCircumferences => Get("Obwody ciała", "Body Circumferences");
        public string Add => Get("Dodaj", "Add");
        public string SaveMeasurements => Get("Zapisz pomiary", "Save Measurements");

        // === Modals ===
        public string ChangePhoto => Get("ZMIEŃ ZDJĘCIE", "CHANGE PHOTO");
        public string FullName => Get("PEŁNE IMIĘ", "FULL NAME");
        public string Cancel => Get("Anuluj", "Cancel");
        public string SaveChanges => Get("Zapisz zmiany", "Save Changes");
        public string OldPassword => Get("STARE HASŁO", "OLD PASSWORD");
        public string NewPassword => Get("NOWE HASŁO", "NEW PASSWORD");
        public string RepeatNewPassword => Get("POWTÓRZ NOWE HASŁO", "REPEAT NEW PASSWORD");
        public string SavePassword => Get("Zapisz hasło", "Save Password");

        // === AuthView ===
        public string AuthSlogan1 => Get("Zasiej nawyki. ", "Plant habits. ");
        public string AuthSlogan2 => Get("Zbierz sukces.", "Harvest success.");
        public string AuthSubtitle => Get("Twój dedykowany tracker produktywności i zdrowia. Bądź konsekwentny, analizuj swoje wyniki i zamieniaj małe kroki w trwałe zmiany stylu życia w zaledwie kilka kliknięć dziennie.", "Your dedicated tracker for productivity and health. Stay consistent, analyze your performance, and turn small steps into lasting lifestyle changes in just a few clicks a day.");
        public string WelcomeBack => Get("Witaj z powrotem", "Welcome back");
        public string SelectAccount => Get("Wybierz konto lub zaloguj się ręcznie.", "Select an account or sign in manually.");
        public string RecentlyActive => Get("OSTATNIO AKTYWNE", "RECENTLY ACTIVE");
        public string EmailAddress => Get("ADRES E-MAIL", "EMAIL ADDRESS");
        public string Password => Get("HASŁO", "PASSWORD");
        public string DontHaveAccount => Get("Nie masz konta?", "Don't have an account?");
        public string RegisterBtn => Get("Zarejestruj się", "Register");
        public string ForgotPassword => Get("Zapomniałeś hasła?", "Forgot password?");
        public string SignIn => Get("Zaloguj się", "Sign In");
        
        public string CreateProfile => Get("Stwórz profil", "Create profile");
        public string SetupAccount => Get("Skonfiguruj swoje konto w kilku szybkich krokach.", "Set up your account in a few quick steps.");
        public string PasswordChecklistMinLength => Get("Przynajmniej 6 znaków", "At least 6 characters");
        public string PasswordChecklistUppercase => Get("Przynajmniej 1 wielka litera", "At least 1 uppercase letter");
        public string PasswordChecklistLowercase => Get("Przynajmniej 1 mała litera", "At least 1 lowercase letter");
        public string PasswordChecklistDigit => Get("Przynajmniej 1 cyfra", "At least 1 digit");
        public string PasswordChecklistSpecial => Get("Przynajmniej 1 znak specjalny", "At least 1 special character");
        public string SignUp => Get("Zarejestruj się", "Sign up");
        public string BackToLogin => Get("Wróć do logowania", "Back to Login");
        
        public string ResetPasswordTitle => Get("Zresetuj hasło", "Reset Password");
        public string EnterEmailForReset => Get("Wprowadź e-mail, aby otrzymać kod", "Enter your email to receive a reset code");
        public string SendResetCode => Get("Wyślij kod resetujący", "Send reset code");
        public string EnterVerificationCode => Get("Wprowadź kod z e-maila", "Enter the verification code from your email");
        public string VerifyCode => Get("Zweryfikuj kod", "Verify code");
        public string CreateNewPassword => Get("Stwórz nowe hasło", "Create your new password");

        // === Week starts ===
        public string Monday => Get("Poniedziałek", "Monday");
        public string Sunday => Get("Niedziela", "Sunday");
    }
}
