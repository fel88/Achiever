using System;
using System.ComponentModel.DataAnnotations;

namespace Achiever.Model
{
    public class UserChallengeInfo
    {
        public int Id { get; set; }
        public Challenge Challenge { get; set; }
        [Required]

        public int ChallengeId { get; set; }
        public User User { get; set; }

        [Required]
        public int UserId { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? CompleteTime { get; set; }
    }
}
