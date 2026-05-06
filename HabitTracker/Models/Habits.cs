using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models{
    [Table("habits")]
    public class Habits:BaseModel{
        [PrimaryKey("id", false)] //false bo Supabase sama wygeneruje UUID
        public string Id {get;set;}

        [Column("user_id")]
        public string UserId {get;set;}

        [Column("category_id")]
        public string CategoryId {get;set;}

        [Column("habit_type_id")]
        public string HabitTypeId{get;set;}

        [Column("name")]
        public string Name{get;set;}

        [Column("period")]
        public string Period{get;set;}

        [Column("target_frequency")]
        public int TargetFrequency {get;set;}

        [Column("days_of_week")]
        public int DaysOfWeek{get;set;}

        [Column("priority")]
        public int Priority{get;set;}

        [Column("is_system")]
        public bool IsSystem{get;set;}

        [Column("is_archived")]
        public bool IsArchived{get;set;}

        [Column("created_date")]
        public DateTime CreatedDate {get;set;}
    
        [Column("isFlexible")]
        public bool IsFlexible{get;set;}

        //Relacje (Foreign Keys)
        [Reference(typeof(Users))]
        public Users User {get;set;}

        [Reference(typeof(HabitCategory))]
        public HabitCategory Category {get;set;}

        [Reference(typeof(HabitTypes))]
        public HabitTypes HabitType {get;set;}
    }
}