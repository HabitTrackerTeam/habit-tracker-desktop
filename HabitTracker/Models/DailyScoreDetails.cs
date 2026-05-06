using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("daily_score_details")]
    public class DailyScoreDetails : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("habit_id")]
        public string HabitId { get; set; }

        [Column("daily_score_id")]
        public string DailyScoreId { get; set; }

        [Column("is_achieved")]
        public bool IsAchieved { get; set; }

        [Reference(typeof(Habits))]
        public Habits Habit { get; set; }

        [Reference(typeof(DailyScores))]
        public DailyScores DailyScore { get; set; }
    }
}