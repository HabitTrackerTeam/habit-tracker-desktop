using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models{
    [Table("habit_logs")]
    public class HabitLogs:BaseModel{
        [PrimaryKey("id", false)]
        public string Id{get;set;}

        [Column("habit_id")]
        public string HabitId{get;set;}

        [Column("log_date")]
        public DateTime LogDate{get;set;}

        [Column("is_completed")]
        public bool IsCompleted{get;set;}

        [Column("numeric_value")]
        public double NumericValue{get;set;}

        [Column("created_date")]
        public DateTime CreatedDate{get;set;}

        [Column("updated_time")]
        public DateTime UpdatedTime{get;set;}

        [Column("status")]
        public int Status{get;set;}

        [Reference(typeof(Habits))]
        public Habits Habit {get;set;}
    }
}