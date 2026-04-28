using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    [Table("body_metrics")]
    public class BodyMetrics : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("measurement_date")]
        public DateTime MeasurementDate { get; set; }

        [Column("weight")]
        public double Weight { get; set; }

        [Column("additional_notes")]
        public string AdditionalNotes { get; set; }

        [Reference(typeof(Users))]
        public Users User { get; set; }
    }
}