using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    [Table("periodic_scores")]
    public class PeriodicScores : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("score_date")]
        public DateTime ScoreDate { get; set; }

        [Column("total_score")]
        public double TotalScore { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("planned_count")]
        public int PlannedCount { get; set; }

        [Column("completed_count")]
        public int CompletedCount { get; set; }

        [Reference(typeof(Users))]
        public Users User { get; set; }
    }
}