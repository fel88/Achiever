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

        public double GetProgressLastDay(AchieverContext ctx, ChallengeAimItem aim)
        {
            var nw = DateTime.UtcNow.Date;

            var p = aim.GetPercentOfAim(ctx, this, nw);
            var p2 = aim.GetPercentOfAim(ctx, this);
            return p2 - p;
        }

        public double GetProgressLastDay(AchieverContext ctx)
        {
            var nw = DateTime.UtcNow.Date;

            var p = GetPercentOfChallenge(ctx, nw);
            var p2 = GetPercentOfChallenge(ctx);

            return p2 - p;
        }

        public double GetPercentOfChallenge(AchieverContext ctx, DateTime? lastFilter = null)
        {
            //UserChallengeInfo chitem2 = ctx.UserChallengeInfos.Include(z => z.Challenge).Include(z => z.Challenge.Aims).Include(z => z.User).FirstOrDefault(z => z.UserId == userId && z.ChallengeId == chId);
            double perctot = 0;

            foreach (var item in Challenge.Aims)
            {
                perctot += item.GetPercentOfAim(ctx, this, lastFilter);
            }
            var ar1 = ctx.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == ChallengeId).ToArray();
            foreach (var item in ar1)
            {
                if (!ctx.UserChallengeInfos.Any(z => z.ChallengeId == item.Child.Id && z.UserId == UserId))
                    continue;

                var fr = ctx.UserChallengeInfos.Where(z => z.ChallengeId == item.Child.Id && z.UserId == UserId).OrderByDescending(z => z.StartTime).FirstOrDefault();
                if (fr == null)
                    continue;

                //todo rethink logic here into chains of UCI

                perctot += fr.GetPercentOfChallenge(ctx, lastFilter);
            }

            perctot /= (Challenge.Aims.Count + ar1.Length);
            //todo: calc all required challenges

            return perctot;
        }

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
                    return;
            }


            IsComplete = true;
            CompleteTime = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
}
