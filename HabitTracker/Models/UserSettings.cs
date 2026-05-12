using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    [Table("user_settings")]
    public class UserSettings : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [Column("language")]
        public string Language { get; set; } = "pl";

        [Column("week_start")]
        public string WeekStart { get; set; } = "monday";

        [Column("units")]
        public string Units { get; set; } = "metric";

        [Column("daily_reminder")]
        public bool DailyReminder { get; set; } = true;

        [Column("reminder_time")]
        public string ReminderTime { get; set; } = "20:00";

        [Column("sound_enabled")]
        public bool SoundEnabled { get; set; } = true;

        [Column("vibration_enabled")]
        public bool VibrationEnabled { get; set; } = false;

        [Column("theme")]
        public string Theme { get; set; } = "light";
    }
}
