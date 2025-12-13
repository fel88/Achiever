using Achiever.Common.Model;
using Achiever.Controllers;
using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Achiever.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CabinetController : ControllerBase
    {
        [HttpGet("/api/[controller]/data/records/week"), Produces("application/json")]
        public async Task<IActionResult> WeekRecordsJson()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            using var context = AchieverContextHolder.GetContext();
            List<object> ret = new List<object>();
            var user = Helper.GetUser(HttpContext.Session);

            var usr = context.Users.Find(Helper.GetUser(HttpContext.Session).Id);
            foreach (var item in context.AchievementItems.Include(z => z.Owner).Where(z => z.OwnerId == null || z.OwnerId == user.Id).ToArray())
            {
                var feats = item.GetFeatures();
                if (feats != null && !feats.IsCumulative)
                    continue;
                var chlds = context.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();

                if (!context.AchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).Any())
                    continue;

                var all = context.AchievementValueItems
                .Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id)))
                .ToArray();

                var frr = all.GroupBy(z =>
                $"{z.Timestamp.Year};{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(z.Timestamp.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}").OrderByDescending(z => z.Sum(u => u.Count)).First();

                var frr2 = all.Where(z =>
                DateTime.Now.Year == z.Timestamp.Year &&
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(z.Timestamp.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) ==
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                );
                var record = frr.Sum(u => u.Count);
                var current = frr2.Sum(z => z.Count);
                ret.Add(new
                {
                    name = item.Name,
                    startTimestamp = frr.Min(z => z.Timestamp).ToLongDateString(),
                    endTimestamp = frr.Max(z => z.Timestamp).ToLongDateString(),
                    record,
                    isRecord = current == record && CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(frr2.First().Timestamp.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) ==
                                            CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                    currentSum = current,
                    remainsToRecord = (record - current),
                    lastRecordTimestamp = all.Any() ? all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o") : "none"
                });
            }

            return Ok(ret);
        }

        [HttpGet("/api/[controller]/data/records/year"), Produces("application/json")]
        public async Task<IActionResult> YearRecordsJson()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            using var context = AchieverContextHolder.GetContext();
            List<object> ret = new List<object>();
            var user = Helper.GetUser(HttpContext.Session);

            var usr = context.Users.Find(Helper.GetUser(HttpContext.Session).Id);

            foreach (var item in context.AchievementItems.Include(z => z.Owner).Where(z => z.OwnerId == null || z.OwnerId == user.Id).ToArray())
            {
                var feats = item.GetFeatures();
                if (feats != null && !feats.IsCumulative) continue;
                var chlds = context.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();

                var all = context.AchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();

                if (!all.Any())
                    continue;

                var frr = all.GroupBy(z => z.Timestamp.Year).OrderByDescending(z => z.Sum(u => u.Count)).First();

                var frr2 = all.Where(z => z.Timestamp.Year == DateTime.UtcNow.Year).ToArray();
                var record = frr.Sum(u => u.Count);
                var current = frr2.Sum(z => z.Count);
                ret.Add(new
                {
                    name = item.Name,
                    year = frr.First().Timestamp.Date.ToString("yyyy"),
                    record,
                    isRecord = current == record && frr.First().Timestamp.Date.Year == DateTime.UtcNow.Year,
                    currentSum = current,
                    remainsToRecord = (record - current),
                    lastRecordTimestamp = all.Any() ? all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o") : "none"
                });

            }
            return Ok(ret);
        }

        [HttpGet("/api/[controller]/data/records/set"), Produces("application/json")]
        public async Task<IActionResult> SetRecordsJson()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            using var context = AchieverContextHolder.GetContext();
            List<object> ret = new List<object>();
            var user = Helper.GetUser(HttpContext.Session);

            var usr = context.Users.Find(Helper.GetUser(HttpContext.Session).Id);

            foreach (var item in context.AchievementItems.Include(z => z.Owner).Where(z => z.OwnerId == null || z.OwnerId == user.Id).ToArray())
            {

                var feats = item.GetFeatures();
                if (feats != null && !feats.IsCumulative)
                    continue;

                dynamic recordStr = 0;
                dynamic record = 0;
                dynamic currentStr = 0;
                dynamic current = 0;
                bool hasCurrent = false;
                bool hasRecord = false;

                DateTime? dts1 = null;

                var chlds = context.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();
                string lastRecordTimestamp = "";
                bool doubled = false;
                if (feats != null)
                {
                    doubled = feats.IsDoubleValued;
                }
                if (doubled)
                {
                    var all = context.DoubleAchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();
                    if (all.Any())
                    {
                        var frr = all.OrderByDescending(z => z.Count * z.Count2).First();
                        var frr2 = all.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).OrderByDescending(z => z.Count * z.Count2).ToArray();
                        var total = frr.Count * frr.Count2 / 100.0;

                        hasRecord = true;
                        record = (int)total;
                        recordStr = $"{frr.Count} x {frr.Count2 / 100.0} = {frr.Count * frr.Count2 / 100.0}";

                        if (frr2.Any())
                        {
                            var fff = frr2.First();
                            current = (int)(fff.Count * fff.Count2 / 100.0);
                            currentStr = $"{fff.Count} x {fff.Count2 / 100.0} = {fff.Count * fff.Count2 / 100.0}";
                            hasCurrent = true;
                        }

                        //current = frr2.Sum(u => u.Count * u.Count2) / 100.0;
                        dts1 = frr.Timestamp;
                        lastRecordTimestamp = all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o");
                    }
                }
                else
                {
                    var all = context.AchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();

                    if (all.Any())
                    {
                        var frr = all.OrderByDescending(z => z.Count).First();
                        var frr2 = all.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).OrderByDescending(z => z.Count).ToArray();

                        hasRecord = true;
                        record = frr.Count;
                        recordStr = frr.Count.ToString();
                        dts1 = frr.Timestamp;
                        if (frr2.Any())
                        {
                            current = frr2.First().Count;
                            currentStr = (frr2.First().Count).ToString();
                            hasCurrent = true;
                        }

                        lastRecordTimestamp = all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o");
                    }
                }

                ret.Add(new
                {
                    id = item.Id,
                    name = item.Name,
                    doubled,
                    hasCurrent,
                    hasRecord,
                    hasDate = dts1.HasValue,
                    date = dts1.HasValue ? ($"{dts1.Value.ToLongDateString()} {dts1.Value.ToLongTimeString()}") : "none",
                    record = recordStr,
                    isRecord = (record == current && dts1.HasValue && dts1.Value.Date == DateTime.UtcNow.Date),
                    currentSum = currentStr,
                    remainsToRecord = record - current,
                    lastRecordTimestamp
                });

            }
            return Ok(ret);
        }

        [HttpGet("/api/[controller]/data/records/day"), Produces("application/json")]
        public async Task<IActionResult> DayRecordsJson()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            using var context = AchieverContextHolder.GetContext();
            List<object> ret = new List<object>();
            var user = Helper.GetUser(HttpContext.Session);

            var usr = context.Users.Find(Helper.GetUser(HttpContext.Session).Id);

            foreach (var item in context.AchievementItems.Include(z => z.Owner).Where(z => z.OwnerId == null || z.OwnerId == user.Id).ToArray())
            {

                var feats = item.GetFeatures();
                if (feats != null && !feats.IsCumulative)
                    continue;

                dynamic record = 0;
                dynamic current = 0;

                DateTime? dts1 = null;

                var chlds = context.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();
                string lastRecordTimestamp = "";
                bool doubled = false;
                if (feats != null)
                {
                    doubled = feats.IsDoubleValued;
                }
                if (doubled)
                {
                    var all = context.DoubleAchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();
                    if (all.Any())
                    {
                        var frr = all.GroupBy(z => $"{z.Timestamp.Year};{z.Timestamp.DayOfYear}").OrderByDescending(z => z.Sum(u => u.Count * u.Count2)).First();
                        var frr2 = all.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).ToArray();
                        record = frr.Sum(u => u.Count * u.Count2) / 100.0;
                        current = frr2.Sum(u => u.Count * u.Count2) / 100.0;
                        dts1 = frr.First().Timestamp.Date;
                        lastRecordTimestamp = all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o");

                    }
                }
                else
                {
                    var all = context.AchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();

                    if (all.Any())
                    {
                        var frr = all.GroupBy(z => $"{z.Timestamp.Year};{z.Timestamp.DayOfYear}").OrderByDescending(z => z.Sum(u => u.Count)).First();
                        var frr2 = all.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).ToArray();
                        record = frr.Sum(u => u.Count);
                        current = frr2.Sum(u => u.Count);
                        dts1 = frr.First().Timestamp.Date;
                        lastRecordTimestamp = all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o");
                    }
                }


                ret.Add(new
                {
                    id = item.Id,
                    name = item.Name,
                    doubled,
                    hasDate = dts1.HasValue,
                    date = dts1.HasValue ? dts1.Value.ToLongDateString() : "none",
                    record,
                    isRecord = (record == current && dts1 == DateTime.UtcNow.Date),
                    currentSum = current,
                    remainsToRecord = record - current,
                    lastRecordTimestamp
                });

            }
            return Ok(ret);
        }
        [HttpGet("/api/[controller]/data/records/month"), Produces("application/json")]
        public async Task<IActionResult> MonthRecordsJson()
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            using var context = AchieverContextHolder.GetContext();
            List<object> ret = new List<object>();
            var user = Helper.GetUser(HttpContext.Session);

            var usr = context.Users.Find(Helper.GetUser(HttpContext.Session).Id);

            foreach (var item in context.AchievementItems.Include(z => z.Owner).Where(z => z.OwnerId == null || z.OwnerId == user.Id).ToArray())
            {
                var feats = item.GetFeatures();
                if (feats != null && !feats.IsCumulative)
                    continue;

                var chlds = context.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();

                var all = context.AchievementValueItems.Where(z => z.User.Id == usr.Id && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id))).ToArray();

                if (!all.Any())
                    continue;

                var frr = all.GroupBy(z =>
                           $"{z.Timestamp.Month};{z.Timestamp.Year}").OrderByDescending(z => z.Sum(u => u.Count)).First();

                var frr2 = all.Where(z => z.Timestamp.Month == DateTime.Now.Month && z.Timestamp.Year == DateTime.Now.Year).ToArray();



                var record = frr.Sum(u => u.Count);
                var current = frr2.Sum(z => z.Count);
                ret.Add(new
                {
                    name = item.Name,
                    date = frr.First().Timestamp.Date.ToString("MMMM yyyy"),
                    record,
                    isRecord = current == record && frr.First().Timestamp.Date.Year == DateTime.UtcNow.Year && frr.First().Timestamp.Date.Month == DateTime.UtcNow.Month,
                    currentSum = current,
                    remainsToRecord = (record - current),
                    lastRecordTimestamp = all.Any() ? all.OrderByDescending(z => z.Timestamp).First().Timestamp.ToString("o") : "none"
                });

            }
            return Ok(ret);
        }

        [HttpGet("/api/[controller]/badge/{id}")]
        public async Task<IActionResult> GetCompetitionBadge(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            using var ctx = AchieverContextHolder.GetContext();
            Console.WriteLine("db path:" + ctx.GetDatabaseFilePath());

            var ch = ctx.Challenges.Find(id);
            if (ch == null)
            {
                return BadRequest();
            }


            if (ch.BadgeSettings == null)
            {
                var str1 = @"<div>
                     " + ch.Name + @"
                     <svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"" width=""10%""><defs><linearGradient gradientTransform=""matrix(1.31117 0 0 1.30239 737.39 159.91)"" gradientUnits=""userSpaceOnUse"" id=""0"" y2=""-.599"" x2=""0"" y1=""45.47""><stop stop-color=""#ffc515"" /><stop offset=""1"" stop-color=""#ffd55b"" /></linearGradient></defs><g transform=""matrix(.85714 0 0 .85714-627.02-130.8)""><path d=""m797.94 212.01l-25.607-48c-.736-1.333-2.068-2.074-3.551-2.074-1.483 0-2.822.889-3.569 2.222l-25.417 48c-.598 1.185-.605 2.815.132 4 .737 1.185 1.921 1.778 3.404 1.778h51.02c1.483 0 2.821-.741 3.42-1.926.747-1.185.753-2.667.165-4"" fill=""url(#0)"" /><path d=""m-26.309 18.07c-1.18 0-2.135.968-2.135 2.129v12.82c0 1.176.948 2.129 2.135 2.129 1.183 0 2.135-.968 2.135-2.129v-12.82c0-1.176-.946-2.129-2.135-2.129zm0 21.348c-1.18 0-2.135.954-2.135 2.135 0 1.18.954 2.135 2.135 2.135 1.181 0 2.135-.954 2.135-2.135 0-1.18-.952-2.135-2.135-2.135z"" transform=""matrix(1.05196 0 0 1.05196 796.53 161.87)"" fill=""#000"" stroke=""#40330d"" 
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             fill-opacity="".75"" /></g></svg>

                     <p>Bagde not setted!</p>
                 </div>";
                return Content(str1, "image/svg+xml; charset=utf-8");

            }

            var b = ResourceFile.GetFileText("badge.svg");
            var txt = Helper.GetText(ch.BadgeSettings);
            var fs1 = Helper.GetFontSize(ch.BadgeSettings);
            var bc1 = Helper.GetBackColor(ch.BadgeSettings);
            var clr = Helper.GetColor(ch.BadgeSettings);
            var fs = 21;
            if (fs1 != null)
            {
                fs = fs1.Value;
            }


            var bdi = new BadgeDrawInfo()
            {
                FontSize = fs,
                BackColor = bc1 == null ? "#ffc600" : bc1,
                Color = clr == null ? "#0000ff" : clr,
                Hardness = Helper.GetHardness(ch.BadgeSettings)
            };
            //replace all
            b = b.Replace("@back1", bdi.BackColor);
            b = b.Replace("@bdi.Color", bdi.Color);
            b = b.Replace("@(bdi.FontSize)", bdi.FontSize.ToString());
            b = b.Replace("@bdi.Hardness", bdi.Hardness.ToString());

            StringBuilder sb = new StringBuilder();
            int yy = 133;
            foreach (var item in txt)
            {
                sb.AppendLine(@"<tspan sodipodi:role=""line""
				   id=""tspan7549""
				   x=""139.83978""
				   y=""" + yy + @""">" + item + "</tspan>");
                yy += 27;

            }
            b = b.Replace("@Lines", sb.ToString());
            b = b.Replace("hidden", "visible");
            /* b = @"<svg>
   <rect id=""box""  fill="""+ bdi.BackColor + @"""  stroke="""+ bdi .Color+ @""" x=""0"" y=""0"" width=""450"" height=""150""/>
 <!-- The Text -->
     <text 
         x=""50%"" 
         y=""50%"" 
         dominant-baseline=""middle"" 
         text-anchor=""middle"" 
         fill=""black"" 
         font-family=""sans-serif"" 
         font-size=""20px"">
         " + txt[0] +@"
     </text>
 </svg>";*/
            return Content(b, "image/svg+xml; charset=utf-8");
        }
    }
}
