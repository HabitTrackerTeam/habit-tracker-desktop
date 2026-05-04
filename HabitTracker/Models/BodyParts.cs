using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("body_parts")]
    public class BodyParts : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("body_part")]
        public string BodyPartName { get; set; } 

        [Column("name")]
        public string Name { get; set; }
        
        public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : BodyPartName;
    }
}