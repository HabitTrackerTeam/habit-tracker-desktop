using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models{
    [Table("habit_types")]
    public class HabitTypes:BaseModel{
        [PrimaryKey("id", false)]
        public string Id{get;set;}

        [Column("type")]
        public string Type {get;set;}

        [Column("display_type")]
        public string DisplayType {get;set;}

        [Column("requires_value")]
        public bool RequiresValue{get;set;}

        [Column("default_unit")]
        public string DefaultUnit{get;set;}
    }
}