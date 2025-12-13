using Achiever.Common.Model;
using Microsoft.EntityFrameworkCore;
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

        public async void CheckAndUpdateComplete(AchieverContext context)
        {
            foreach (var aim in Challenge.Aims)
            {
                bool compl = aim.IsAimAchieved(context, this, User);

                if (!compl)
                {
                    return;
                }
            }

            var a1 = context.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == ChallengeId).ToArray();
            foreach (var zz in a1)
            {
                //todo strange logic here. think further
                if (!zz.Child.IsComplete(context, UserId))
                    return ;
            }

            
            IsComplete = true;
            CompleteTime = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
}
