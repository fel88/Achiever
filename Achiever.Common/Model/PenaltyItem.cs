namespace Achiever.Model
{
    public class PenaltyItem
    {
        public int Id { get; set; }
        public AchievementItem Achievement { get; set; }
        public int Days { get; set; }
        public decimal Modifier { get; set; }
        public bool IsCumulative { get; set; }
    }
}
