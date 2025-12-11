using System;

namespace Achiever.Model
{
    public class DoubleAchievementValueItem 
    {
        public int Id { get; set; }
        public AchievementItem Achievement { get; set; }
        public User User { get; set; }

        public DateTime Timestamp { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
        public int Count2 { get; set; }
        public string XmlConfig { get; set; }

    }
}
