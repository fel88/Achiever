namespace Achiever.Model
{
    public class ChallengeRequirement
    {
        public int Id { get; set; }
        public Challenge Parent { get; set; }
        public Challenge Child { get; set; }

        /// <summary>
        /// незаконченное child испытание блокирует начатие родительского испытания
        /// </summary>
        public bool StartBlocking { get; set; }
    }
}
