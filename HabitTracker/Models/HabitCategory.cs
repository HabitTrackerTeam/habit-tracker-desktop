using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HabitTracker.Models{
    [Table("habit_categories")]
    public class HabitCategory:BaseModel{
        [PrimaryKey("id",false)]
        public string Id{get;set;}

        [Column("user_id")]
        public string UserId{get;set;}

        [Column("name")]
        public string Name{get;set;}

        [Column("color_id")]
        public string ColorId{get;set;}

        //Relacje
        [Reference(typeof(Users))]
        public Users User{get;set;}

        [Reference(typeof(Colors))]
        public Colors Color{get;set;}
    }
}