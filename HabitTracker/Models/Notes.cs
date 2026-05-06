using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    [Table("notes")]
    public class Notes : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("note_date")]
        public DateTime NoteDate { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Reference(typeof(Users))]
        public Users User { get; set; }
    }
}