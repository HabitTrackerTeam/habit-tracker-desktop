using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models{
    [Table("colors")]
    public class Colors:BaseModel{
        [PrimaryKey("id", false)]
        public string Id{get;set;}

        [Column("color_code")]
        public string ColorCode{get;set;}

        [Column("name")]
        public string Name{get;set;}
    }
}