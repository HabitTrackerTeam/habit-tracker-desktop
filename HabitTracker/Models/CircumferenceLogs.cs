using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("circumference_logs")]
    public class CircumferenceLogs : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("session_id")]
        public string SessionId { get; set; }

        [Column("value")]
        public double Value { get; set; }

        [Column("body_part_id")]
        public string BodyPartId { get; set; }

        [Reference(typeof(MeasurementSessions))]
        public MeasurementSessions Session { get; set; }

        [Reference(typeof(BodyParts))]
        public BodyParts BodyPart { get; set; }
    }
}