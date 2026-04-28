using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("periodic_score_details")]
    public class PeriodicScoreDetails : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("habit_id")]
        public string HabitId { get; set; }

        [Column("periodic_score_id")]
        public string PeriodicScoreId { get; set; }

        [Column("planned_count")]
        public int PlannedCount { get; set; }

        [Column("completed_count")]
        public int CompletedCount { get; set; }

        [Column("is_achieved")]
        public bool IsAchieved { get; set; }

        [Reference(typeof(Habits))]
        public Habits Habit { get; set; }

        [Reference(typeof(PeriodicScores))]
        public PeriodicScores PeriodicScore { get; set; }
    }
}