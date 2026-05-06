using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    //przekazanie Supabase, ze ten model odpowiada tabeli users
    [Table("users")]
    public class Users: BaseModel //udostepnia funkcje systemowe: Insert() itd.
    {
        [PrimaryKey("id")]
        public string Id {get;set;}

        [Column("name")]
        public string Name {get; set;}
    }
}