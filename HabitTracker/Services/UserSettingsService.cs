using System;
using System.Linq;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public static class UserSettingsService
    {
        public static async Task<UserSettings> LoadSettingsAsync(string userId)
        {
            var response = await SupabaseService.Client
                .From<UserSettings>()
                .Where(s => s.UserId == userId)
                .Get();

            var settings = response.Models.FirstOrDefault();

            if (settings != null)
                return settings;

            var currentUser = SupabaseService.Client.Auth.CurrentUser;
            string initialNickname = string.Empty;
            if (currentUser?.UserMetadata != null && currentUser.UserMetadata.ContainsKey("nickname"))
                initialNickname = currentUser.UserMetadata["nickname"]?.ToString() ?? string.Empty;

            var defaults = new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Nickname = initialNickname,
                Language = "pl",
                WeekStart = "monday",
                Units = "metric",
                DailyReminder = true,
                ReminderTime = "20:00",
                SoundEnabled = true,
                VibrationEnabled = false,
                Theme = "light"
            };

            await SupabaseService.Client.From<UserSettings>().Insert(defaults);

            var insertedResponse = await SupabaseService.Client
                .From<UserSettings>()
                .Where(s => s.UserId == userId)
                .Get();

            return insertedResponse.Models.FirstOrDefault() ?? defaults;
        }

        public static async Task SaveSettingsAsync(UserSettings settings)
        {
            await SupabaseService.Client
                .From<UserSettings>()
                .Upsert(settings);
        }
    }
}
