using System;

namespace UntitledJBTClone.Game.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastPlayedAt { get; set; }
        public int TotalScore { get; set; }
        public int PlayCount { get; set; }
        public string Avatar { get; set; } = "default";

        public Profile()
        {
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
        }

        public Profile(string name) : this()
        {
            Name = name;
        }
    }
}