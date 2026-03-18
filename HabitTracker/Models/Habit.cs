using Postgrest.Attributes;
using Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("habits")] // Nazwa tabeli w Supabase
    public class Habit : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }
    }
}