using System.ComponentModel.DataAnnotations;

namespace Achiever.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public string Login { get; set; }
        public string AvatarPath { get; set; }

        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public int PaidPeriod { get; set; }
        public bool GoldUser { get; set; }
        public bool Enabled { get; set; }
        public string XmlConfig { get; set; }
        public long? TelegramChatId { get; set; }
    }
}
