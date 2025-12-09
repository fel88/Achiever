using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Color = System.Drawing.Color;

namespace Achiever.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {

        [HttpPost("/api/[controller]/item")]
        public async Task<IActionResult> NewItem(/*NewChallengeDto dto*/)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            //if (dto == null) return BadRequest();
            //if (string.IsNullOrWhiteSpace(dto.name)) return BadRequest();

            var name = Request.Form["name"];
            var desc = Request.Form["desc"];
            bool cumulative = false;

            if (Request.Form.ContainsKey("noncumulative"))
            {
                cumulative = Request.Form["noncumulative"] == "on";
            }

            bool singular = false;
            if (Request.Form.ContainsKey("singular"))
            {
                singular = Request.Form["singular"] == "on";
            }
            bool doubleValued = false;
            if (Request.Form.ContainsKey("doubled"))
            {
                doubleValued = Request.Form["doubled"] == "on";
            }
            var ctx = new AchieverContext();

            List<string> flist = new List<string>();
            if (singular)
            {
                flist.Add("'signular'");
            }
            if (doubleValued)
            {
                flist.Add("'doubleValued'");
            }
            List<string> jsonList = new List<string>();
            string sing = " features:[" + string.Join(",", flist) + "]";
            jsonList.Add(sing);
            if (cumulative)
            {
                jsonList.Add("cumulative: 0");
            }
            var stg = "{" + string.Join(",", jsonList) + "}";


            /*string stg = string.Empty;
            if (cumulative)
            {
                stg = cuml;
            }
            if (singular)
            {
                stg = sing;
            }*/

            ctx.AchievementItems.Add(new AchievementItem()
            {
                Name = name,
                Description = desc,
                Settings = stg,
                OwnerId = Helper.GetUser(HttpContext.Session).Id
            });

            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/api/[controller]/challenge/item")]
        public async Task<IActionResult> NewChallengeItem()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            string body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var obj = JObject.Parse(body);
            var cid = int.Parse(obj["challengeId"].ToString());
            var iid = int.Parse(obj["itemId"].ToString());
            var count = int.Parse(obj["count"].ToString());
            var type = int.Parse(obj["type"].ToString());

            string stg = null;
            AimType tp = AimType.Count;
            switch (type)
            {
                case 0:

                    break;
                case 1:
                    {
                        tp = AimType.Bool;
                        var mincnt = int.Parse(obj["minCount2"].ToString());
                        if (mincnt <= 1)
                        {
                            stg = @"{
	'period':'set'
}";
                        }
                        else
                        {
                            stg = @"{
	'period':'set',
'set': {
		'constraints':{
			'amount': " + mincnt + @"
                      }
           }
}";
                        }
                    }
                    break;
                case 3:
                    {
                        tp = AimType.Bool;
                        stg = @"{
	                        'period':'month'
                        }";
                    }
                    break;
                case 4:
                    {
                        tp = AimType.Bool;
                        stg = @"{
	                        'period':'year'
                        }";
                    }
                    break;
                case 2:
                    {
                        var mincnt = int.Parse(obj["minCount"].ToString());
                        var daysCount = int.Parse(obj["daysCount"].ToString());
                        tp = AimType.Bool;
                        stg = @"{
	'period': 'day',";
                        if (daysCount > 1)
                            stg += "'times': " + daysCount + ",";

                        stg += @"
	'set': {
		'constraints':{
			'minCount': " + mincnt + @",";

                        stg += "}}}";
                    }
                    break;

            }

            var ctx = new AchieverContext();
            var chl = ctx.Challenges.Find(cid);
            chl.Aims.Add(new ChallengeAimItem()
            {
                AchievementId = iid,
                Count = count,
                //Type = AimType.Count,
                Type = tp,
                Settings = stg
            });
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/api/[controller]/challenge/required")]
        public async Task<IActionResult> NewChallengeRequired()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            string body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var obj = JObject.Parse(body);
            var cid = int.Parse(obj["challengeId"].ToString());
            var iid = int.Parse(obj["itemId"].ToString());


            var ctx = new AchieverContext();
            var chl = ctx.Challenges.Find(cid);
            ctx.ChallengeRequirements.Add(new ChallengeRequirement()
            {
                Parent = chl,
                Child = ctx.Challenges.Find(iid)
            });

            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("/api/[controller]/checkin/{id}")]
        public async Task<IActionResult> ChallengeCheckIn(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var user = Helper.GetUser(HttpContext.Session);
            var ctx = new AchieverContext();
            ctx.UserChallengeInfos.Add(new UserChallengeInfo()
            {
                UserId = user.Id,
                ChallengeId = id,
                StartTime = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("/api/[controller]/item/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var fr = ctx.AchievementItems.Find(id);
            ctx.AchievementItems.Remove(fr);
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("/api/[controller]/challenge/item/{chId}/{achId}")]
        public async Task<IActionResult> DeleteChallengeItem(int chId, int achId)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var fr = ctx.Challenges.Find(chId);

            var user = Helper.GetUser(HttpContext.Session);
            if (fr.OwnerId != user.Id) return BadRequest();

            var f2 = ctx.ChallengeAimItems.Find(achId);
            ctx.ChallengeAimItems.Remove(f2);

            await ctx.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("/api/[controller]/challenge/req/{chId}/{reqId}")]
        public async Task<IActionResult> DeleteChallengeReq(int chId, int reqId)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var fr = ctx.Challenges.Find(chId);

            var user = Helper.GetUser(HttpContext.Session);
            if (fr.OwnerId != user.Id) return BadRequest();

            var f2 = ctx.ChallengeRequirements.Find(reqId);
            ctx.ChallengeRequirements.Remove(f2);

            await ctx.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("/api/[controller]")]
        public async Task<IActionResult> NewChallenge(/*NewChallengeDto dto*/)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            var ctx = new AchieverContext();

            var cnt = ctx.Challenges.Count(z => z.OwnerId == user.Id);

            //check membershipPlan
            if (!(user.GoldUser || user.PaidPeriod > 0))
            {
                if (cnt >= 3)
                {
                    return BadRequest();
                }
            }


            /*monetizeation here.
             * 
             */

            //if (dto == null) return BadRequest();
            //if (string.IsNullOrWhiteSpace(dto.name)) return BadRequest();

            var name = Request.Form["name"];
            var desc = Request.Form["desc"];

            ctx.Challenges.Add(new Challenge()
            {
                Name = name,
                Description = desc,
                OwnerId = Helper.GetUser(HttpContext.Session).Id
            });
            await ctx.SaveChangesAsync();
            return Ok();
        }

        public static Random r = new Random();
        [HttpPatch("/api/[controller]/tracking/{id}/{type}")]
        public async Task<IActionResult> ChangeTrackingChallenge(int id, string type)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Include(z => z.Aims).SingleOrDefault(z => z.Id == id);

            if (type == "alltime")
            {
                ch.UseValuesAfterStartOnly = false;
            }
            else
            {
                ch.UseValuesAfterStartOnly = true;
            }

            await ctx.SaveChangesAsync();
            return Ok();
        }
     
        public static string[] Tokenize(string data)
        {
            List<string> ret = new List<string>();
            bool insideString = false;
            StringBuilder sb = new StringBuilder();
            char[] symbols = { '=', ';', '(', ')', ',', ':', '}', '{' };
            for (int i = 0; i < data.Length; i++)
            {
                if (!insideString && (data[i] == '\n' || data[i] == '\r'))
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }
                if (data[i] == '\'')
                {
                    insideString = !insideString;
                    if (!insideString)
                    {
                        sb.Append('\'');
                        ret.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                if (!insideString)
                    if (symbols.Contains(data[i]))
                    {
                        if (sb.Length > 0)
                        {
                            ret.Add(sb.ToString());
                            sb.Clear();
                        }
                        ret.Add(data[i].ToString());
                        continue;
                    }

                sb.Append(data[i]);
            }
            if (sb.Length > 0)
            {
                ret.Add(sb.ToString());
            }
            return ret.Select(z => z.Trim()).Where(z => z.Length > 0).ToArray();
        }
        [HttpPatch("/api/[controller]/badge/")]
        public async Task<IActionResult> ChangeBadgeChallenge(BadgeFontPatchDto dto)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Include(z => z.Aims).SingleOrDefault(z => z.Id == dto.BadgeId);


            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach (var item in ch.Name.Split('.').ToArray())
            {
                sb.Append($"'{item}',");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            //var arr1 = ch.BadgeSettings.Split(new char[] { ':', ' ', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var arr1 = Tokenize(ch.BadgeSettings);
            int fs = 16;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i].Contains("fontSize"))
                {

                    fs = int.Parse(arr1[i + 2]);
                    arr1[i + 2] = (fs + dto.Step).ToString();
                }
            }
            ch.BadgeSettings = string.Join(string.Empty, arr1);
            //            ch.BadgeSettings = @"{
            //'text': " + sb.ToString() + @",
            //	'backColor': " + $"'#{clr1.ToArgb():X}'" + @",
            //	'color': " + $"'#{clr2.ToArgb():X}'" + @",
            //'fontSize': 16
            //}";
            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/badge_clrs/")]
        public async Task<IActionResult> ChangeBadgeColorsChallenge(BadgeColorsPatchDto dto)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            if (dto.Fore == null || dto.Back == null) return BadRequest();

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Include(z => z.Aims).SingleOrDefault(z => z.Id == dto.BadgeId);



            var arr1 = Tokenize(ch.BadgeSettings);

            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i].Contains("backColor"))
                {
                    arr1[i + 2] = $"'{dto.Back}'";
                }
                if (arr1[i].Contains("color"))
                {
                    arr1[i + 2] = $"'{dto.Fore}'";
                }
            }
            ch.BadgeSettings = string.Join(string.Empty, arr1);

            await ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("/api/[controller]/badge_rnd/{id}")]
        public async Task<IActionResult> RandomChangeBadgeChallenge(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Include(z => z.Aims).SingleOrDefault(z => z.Id == id);

            var clr1 = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
            var clr2 = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));

            int counter = 0; 
            var brightness = clr1.GetBrightness();

             //clr2 = brightness > 0.5 ? Color.Black : Color.White;
            while (ColorContrastCalculator.GetContrastRatio(clr1, clr2) < 5)
            {
                clr1 = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
                clr2 = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
                //counter++;
              //  if (counter > 100)
                  //  break;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach (var item in ch.Name.Split('.').ToArray())
            {
                sb.Append($"'{item}',");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            ch.BadgeSettings = @"{
'text': " + sb.ToString() + @",
	'backColor': " + $"'#{clr1.R:X2}{clr1.G:X2}{clr1.B:X2}{clr1.A:X2}'" + @",
	'color': " + $"'#{clr2.R:X2}{clr2.G:X2}{clr2.B:X2}{clr2.A:X2}'" + @",
'fontSize': 16
}";
            await ctx.SaveChangesAsync();
            var data = new { ContrastRatio = ColorContrastCalculator.GetContrastRatio(clr1, clr2) };            
            return Ok(data);
            
        }
        [HttpDelete("/api/[controller]/{id}")]
        public async Task<IActionResult> DeleteChallenge(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Include(z => z.Aims).SingleOrDefault(z => z.Id == id);
            var huser = Helper.GetUser(HttpContext.Session);
            var user = ctx.Users.Find(huser.Id);
            var uci = ctx.UserChallengeInfos.SingleOrDefault(z => z.UserId == user.Id && z.ChallengeId == ch.Id);
            if (uci != null)
            {
                ctx.UserChallengeInfos.Remove(uci);
            }
            foreach (var item in ch.Aims)
            {
                ctx.ChallengeAimItems.Remove(item);
            }
            ctx.Challenges.Remove(ch);
            await ctx.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("/api/[controller]/leave/{id}")]
        public async Task<IActionResult> LeaveChallenge(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Challenges.Find(id);
            var huser = Helper.GetUser(HttpContext.Session);
            var user = ctx.Users.Find(huser.Id);
            var uci = ctx.UserChallengeInfos.SingleOrDefault(z => z.UserId == user.Id && z.ChallengeId == ch.Id);
            ctx.UserChallengeInfos.Remove(uci);
            await ctx.SaveChangesAsync();

            return Ok();
        }
    }
}