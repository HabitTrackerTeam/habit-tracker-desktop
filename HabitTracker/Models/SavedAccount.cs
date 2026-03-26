using System;

namespace HabitTracker.Models
{
    public class SavedAccount
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        
        public DateTime LastLogin { get; set; } 
    }
}