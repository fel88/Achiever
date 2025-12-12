using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Achiever.Common.Model;
using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Achiever.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {

        [HttpGet]
        [Route("{infoId}")]
        public async Task<ActionResult<UserInfoDto>> GetInfo(int infoId)
        {
            AchieverContext context = AchieverContextHolder.GetContext();

            var sng = await context.UserChallengeInfos.Include(z => z.Challenge).Include(z => z.Challenge.Aims).SingleOrDefaultAsync(z => z.Id == infoId);
            List<string> ss = new List<string>();
            foreach (var item in sng.Challenge.Aims)
            {
                var item1 = context.AchievementItems.SingleOrDefault(z => z.Id == item.AchievementId);
                var user = Helper.GetUser(HttpContext.Session);
                var ret = Helper.GetAimLabels(context, sng, item, user);
                //ss.Add(item1.Name + " - кол-во: " + item.Count);
                ss.Add(ret.Title);
            }
            return new UserInfoDto()
            {
                //Complete = sng.CompleteTime.Value,
                Complete = $"{sng.CompleteTime.Value.ToLongDateString()} {sng.CompleteTime.Value.ToLongTimeString()}",
                Text = ss.ToArray(),
                Name = sng.Challenge.Name
            };
        }

      

        [HttpPost]
        public async Task<IActionResult> AddActivity(ActivityAddDto activityAddDto)//AchievementItem achive, [FromQuery] int count
        {
            AchieverContext context = AchieverContextHolder.GetContext();

            var user = Helper.GetUser(HttpContext.Session);
            if (user == null)
            {
                return Unauthorized();
            }
            //var user = context.Users.First();
            user = context.Users.Find(user.Id);
            var a = context.AchievementItems.Single(z => z.Id == activityAddDto.AchieveId);
            context.AchievementValueItems.Add(new AchievementValueItem()
            {
                Timestamp = DateTime.Now,
                Count = activityAddDto.Count,
                Achievement = a,
                User = user
            });

            //check if challenge complete?

            await context.SaveChangesAsync();
            Helper.CheckAllChallenges(user.Id);

            return NoContent();
        }
                
        [HttpPost("/api/[controller]/doubled")]
        public async Task<IActionResult> AddDoubledActivity(ActivityDoubledAddDto activityAddDto)//AchievementItem achive, [FromQuery] int count
        {
            AchieverContext context = AchieverContextHolder.GetContext();

            var user = Helper.GetUser(HttpContext.Session);
            if (user == null)
            {
                return Unauthorized();
            }
            //var user = context.Users.First();
            user = context.Users.Find(user.Id);
            var a = context.AchievementItems.Single(z => z.Id == activityAddDto.AchieveId);
            context.DoubleAchievementValueItems.Add(new DoubleAchievementValueItem()
            {
                Timestamp = DateTime.Now,
                Count = activityAddDto.Count,
                Count2 = activityAddDto.Count2,
                Achievement = a,
                User = user
            });

            //check if challenge complete?

            await context.SaveChangesAsync();
            Helper.CheckAllChallenges(user.Id);

            return NoContent();
        }

    }
}