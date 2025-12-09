using Achiever.Common.Model;
using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achiever.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("/api/[controller]/backup")]
        public IActionResult Backup()
        {
            try
            {

                return PhysicalFile(Path.Combine(Startup.RootPath, "..", "achiever.db"), "application/octet-stream", "achiever_backup.db");
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
            return BadRequest();
        }

        [HttpPatch("/api/[controller]/challenge/owner/reset/{id}")]
        public async Task<IActionResult> ResetChallengeOwner(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.Challenges.Find(id);
            uu.OwnerId = null;
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/aim/owner/reset/{id}")]
        public async Task<IActionResult> ResetAimOwner(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.AchievementItems.Find(id);
            uu.OwnerId = null;
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/user/enabled/switch/{id}")]
        public async Task<IActionResult> UserEnabledSwitch(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.Users.Find(id);
            uu.Enabled = !uu.Enabled;
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/challenge/owner/set/{id}/{ownerId}")]
        public async Task<IActionResult> SetChallengeOwner(int id, int ownerId)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();


            var ctx = new AchieverContext();
            var user2 = ctx.Users.Find(ownerId);
            if (user2 == null) return BadRequest();

            var uu = ctx.Challenges.Find(id);
            uu.OwnerId = ownerId;
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/subAchievement/parent/set/{aId}/{parentId}")]
        public async Task<IActionResult> SetAchievementItemParent(int aId, int parentId)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin)
                return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.AchievementItems.Find(aId);
            uu.Parent = ctx.AchievementItems.Find(parentId);
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/aim/owner/set/{id}/{ownerId}")]
        public async Task<IActionResult> SetAimOwner(int id, int ownerId)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();


            var ctx = new AchieverContext();
            var user2 = ctx.Users.Find(ownerId);
            if (user2 == null) return BadRequest();

            var uu = ctx.AchievementItems.Find(id);
            uu.OwnerId = ownerId;
            await ctx.SaveChangesAsync();
            return Ok();
        }


        [HttpPatch("/api/[controller]/user/gold/{id}/{val}")]
        public async Task<IActionResult> ChangeUserGoldStatus(int id, int val)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.Users.Find(id);
            uu.GoldUser = val == 1;

            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/challenge/name")]
        public async Task<IActionResult> ChangeChallengeName(CommonDataDto dto)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin) return Unauthorized();

            var ctx = new AchieverContext();

            var uu = ctx.Challenges.Find(dto.itemId);
            if (string.IsNullOrEmpty(dto.value)) return BadRequest();
            uu.Name = dto.value;
            dynamic stuff = JsonConvert.DeserializeObject(uu.BadgeSettings);

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach (var item in uu.Name.Split('.').ToArray())
            {
                sb.Append($"\"{item}\",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");


            stuff.text = JArray.Parse(sb.ToString());

            uu.BadgeSettings = JsonConvert.SerializeObject(stuff);

            await ctx.SaveChangesAsync();
            return Ok();
        }
    }
}
