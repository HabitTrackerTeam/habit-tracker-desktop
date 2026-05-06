using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models{
    [Table("reminders")]
    public class Reminders:BaseModel{
        [PrimaryKey("id", false)]
        public string Id{get;set;}

        [Column("habit_id")]
        public string HabitId{get;set;}

        [Column("time_of_day")]
        public TimeSpan TimeOfDay{get;set;}

        [Column("days_of_week")]
        public int DaysOfWeek{get;set;}

        [Column("is_active")]
        public bool IsActive{get;set;}

        [Reference(typeof(Habits))]
        public Habits Habits{get;set;}
    }
}