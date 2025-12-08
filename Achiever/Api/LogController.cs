using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace Achiever.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        [HttpDelete("/api/[controller]/{id}")]
        public async Task<IActionResult> DeleteLogItem(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.AchievementValueItems.Find(id);
            ctx.AchievementValueItems.Remove(ch);
            await ctx.SaveChangesAsync();
            return Ok();
        }

    

        [HttpPatch("/api/[controller]/time/{id}")]
        public async Task<IActionResult> PatchItemDateTime(int id, [FromBody] DateTimeDto dto)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            if (bool.Parse(dto.doubled))
            {
                var ch = ctx.DoubleAchievementValueItems.Find(id);
                ch.Timestamp = new DateTime(dto.year, dto.month, dto.day, dto.hours, dto.minutes, dto.seconds);
            }
            else
            {
                var ch = ctx.AchievementValueItems.Find(id);
                ch.Timestamp = new DateTime(dto.year, dto.month, dto.day, dto.hours, dto.minutes, dto.seconds);
            }
            await ctx.SaveChangesAsync();
            return Ok();
        }

        

        [HttpGet("/api/[controller]/pages/{qtyPerPage}")]
        public async Task<IActionResult> GetPagesCount(int qtyPerPage)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
                return Unauthorized();

            var context = new AchieverContext();
            var cnt = context.AchievementValueItems.Count();
            return new JsonResult(new ValueDto() { value = ((cnt / qtyPerPage) + 1).ToString() });
        }

        [HttpGet("/api/[controller]/list/{page}")]
        public async Task<IActionResult> Get(int page)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            var context = new AchieverContext();
            var ret = new List<LogDto>();
            foreach (var item in context.AchievementValueItems.Where(z => z.User.Id == user.Id).Include(z => z.Achievement).OrderByDescending(z => z.Timestamp))
            {
                ret.Add(new LogDto()
                {
                    id = item.Id,
                    aId = item.Achievement.Id,
                    count = item.Count,
                    timestamp = item.Timestamp,
                    desc = item.Description,
                    penalty = (int)(item.Count - Helper.GetModifier(item.Timestamp) * item.Count)
                });
            }
            return new JsonResult(new { items = ret, names = context.AchievementItems.Select(z => new { name = z.Name, id = z.Id }) });
        }
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        [HttpGet("/api/[controller]/list/period/{period}")]
        public async Task<IActionResult> Get(string period)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            var context = new AchieverContext();
            var ret = new List<LogDto>();
            var filter = context.AchievementValueItems
                .Where(z => z.User.Id == user.Id);

            var filterd = context.DoubleAchievementValueItems
              .Where(z => z.User.Id == user.Id);

            if (period == "today")
            {
                filter = filter.Where(z => z.Timestamp.Date == DateTime.Now.Date);
                filterd = filterd.Where(z => z.Timestamp.Date == DateTime.Now.Date);
            }
            if (period == "yesterday")
            {
                filter = filter.Where(z => z.Timestamp.Date == DateTime.Now.Date.AddDays(-1));
                filterd = filterd.Where(z => z.Timestamp.Date == DateTime.Now.Date.AddDays(-1));
            }
            if (period == "year")
            {
                filter = filter.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year);
                filterd = filterd.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year);
            }
            if (period == "month")
            {
                filter = filter.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year && z.Timestamp.Date.Month == DateTime.Now.Date.Month);
                filterd = filterd.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year && z.Timestamp.Date.Month == DateTime.Now.Date.Month);
            }

            filter = filter
                .Include(z => z.Achievement)
                .OrderByDescending(z => z.Timestamp);

            filterd = filterd
                .Include(z => z.Achievement)
                .OrderByDescending(z => z.Timestamp);

            var filter2 = filter.ToArray();
            var filter2d = filterd.ToArray();

            if (period == "week")
            {
                filter2 = filter2.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year &&
                GetIso8601WeekOfYear(z.Timestamp.Date) == GetIso8601WeekOfYear(DateTime.Now.Date)).ToArray();
                filter2d = filter2d.Where(z => z.Timestamp.Date.Year == DateTime.Now.Date.Year &&
              GetIso8601WeekOfYear(z.Timestamp.Date) == GetIso8601WeekOfYear(DateTime.Now.Date)).ToArray();
            }

            foreach (var item in filter2)
            {
                ret.Add(new LogDto()
                {
                    id = item.Id,
                    aId = item.Achievement.Id,
                    count = item.Count,
                    timestamp = item.Timestamp,
                    desc = item.Description,
                    penalty = (int)(item.Count - Helper.GetModifier(item.Timestamp) * item.Count)
                });
            }
            foreach (var item in filter2d)
            {
                ret.Add(new LogDto()
                {
                    id = item.Id,
                    doubled = true,
                    count2 = item.Count2,
                    aId = item.Achievement.Id,
                    count = item.Count,
                    timestamp = item.Timestamp,
                    desc = item.Description,
                    penalty = (int)(item.Count - Helper.GetModifier(item.Timestamp) * item.Count)
                });
            }
            return new JsonResult(new { items = ret, names = context.AchievementItems.Select(z => new { name = z.Name, id = z.Id }) });
        }

        [HttpPatch("/api/[controller]/desc/{id}")]
        public async Task<IActionResult> PatchItemDesc(int id, [FromBody] DescPatchDto dto)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.AchievementValueItems.Find(id);
            ch.Description = dto.desc;

            await ctx.SaveChangesAsync();
            return Ok();
        }
    }
}