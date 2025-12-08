using System;

namespace Achiever.Model
{
    public class AchievementValueItem
    {
        public int Id { get; set; }
        public AchievementItem Achievement { get; set; }
        public User User { get; set; }

        public DateTime Timestamp { get; set; }
        public int Count { get; set; }        
        public string Description { get; set; }
    }
}
