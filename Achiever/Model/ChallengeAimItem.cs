using System;

namespace Achiever.Model
{
    public class ChallengeAimItem
    {
        public int Id { get; set; }
        public int AchievementId { get; set; }
        public AchievementItem Achievement { get; set; }

        public DateTime? UntilDate { get; set; }
        public int? Count { get; set; }//target count
        public int? DaysPeriod { get; set; }
        public int? MinPerDayCount { get; set; }
        public int? MaxDaysGap { get; set; }

        public AimType Type { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// json settings
        /// </summary>
        public string Settings { get; set; }

    }
}
