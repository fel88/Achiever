using Achiever.Common.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Achiever.Model
{
    public class Challenge
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// статичная дата завершения.
        /// </summary>
        public DateTime? UntilDate { get; set; }
        public List<ChallengeAimItem> Aims { get; set; } = new List<ChallengeAimItem>();

        /// <summary>
        /// json with badge settings: color, hardness, etc
        /// </summary>
        public string BadgeSettings { get; set; }

        /// <summary>
        /// если true  то будут учитываться только достижения с начала принятия участия в состязании , а не все подряд
        /// </summary>
        public bool UseValuesAfterStartOnly { get; set; }
        public bool IsRenewable() => UseValuesAfterStartOnly;
        public bool IsExpired() => UntilDate == null ? false : UntilDate.Value >= DateTime.UtcNow;

        public  bool IsComplete(AchieverContext context, int userId)
        {            
            var user = context.Users.Find(userId);

            var userInfos = context.UserChallengeInfos.Where(z => z.ChallengeId == Id && userId == z.UserId).Include(z => z.Challenge).Include(z => z.Challenge.Aims).ToArray();
            if (!userInfos.Any())
                return false;

            if (userInfos.Any(z => z.IsComplete))
                return true;

            var item = userInfos.Where(z => !z.IsComplete).First();

            foreach (var aim in item.Challenge.Aims)
            {
                bool compl = aim.IsAimAchieved(context, item, user);

                if (!compl)
                {
                    return false;
                }
            }

            var a1 = context.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == Id).ToArray();
            foreach (var zz in a1)
            {
                if (!zz.Child.IsComplete(context, userId))
                    return false;
            }

            return true;

        }

        /// <summary>
        /// длительноть в часах с момента принятия участия
        /// </summary>
        public int? Duration { get; set; }

        public int? OwnerId { get; set; }
        public User Owner { get; set; }
        public string XmlConfig { get; set; }
    }
}
