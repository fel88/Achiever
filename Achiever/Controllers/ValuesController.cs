using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Achiever.Common.Model;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace Achiever.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public object GetDayReport(int monthId, int yearId, int id)
        {
            AchieverContext context = AchieverContextHolder.GetContext();
            var user = Helper.GetUser(HttpContext.Session);

            var aa = context.AchievementValueItems.Include(z => z.User).Where(z =>
              z.User.Id == user.Id && z.Timestamp.Month == monthId && z.Timestamp.Year == yearId && z.Achievement.Id == id).ToArray().GroupBy(z => z.Timestamp.DayOfYear);
            int days = DateTime.DaysInMonth(DateTime.Now.Year, monthId);
            List<int> vv = new List<int>();
            for (int i = 0; i < days; i++)
            {
                vv.Add(0);
            }
            foreach (var item in aa)
            {
                var sum = item.Sum(z => z.Count);
                vv[item.First().Timestamp.Day - 1] = sum;
            }

            var ach = context.AchievementItems.First(z => z.Id == id);
            var dt = new DateTime(yearId, monthId, 1);
            List<string> ss = new List<string>();
            while (dt.Month == monthId)
            {
                ss.Add(dt.ToLongDateString());
                dt = dt.AddDays(1);
            }
            return new { data = vv, name = ach.Name, labels = ss.ToArray() };

        }
        public object GetMonthReport(int yearId, int id)
        {
            AchieverContext context = AchieverContextHolder.GetContext();
            var user = Helper.GetUser(HttpContext.Session);

            var aa = context.AchievementValueItems.Include(z => z.User).Where(z =>
                 z.User.Id == user.Id && z.Timestamp.Year == yearId && z.Achievement.Id == id).ToArray().GroupBy(z => z.Timestamp.Month).ToArray();

            List<int> vv = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                vv.Add(0);
            }

            foreach (var item in aa)
            {
                var sum = item.Sum(z => z.Count);
                vv[item.First().Timestamp.Month - 1] = sum;
            }

            var ach = context.AchievementItems.First(z => z.Id == id);
            var dt = new DateTime(yearId, 1, 1);
            List<string> ss = new List<string>();
            while (dt.Year == yearId)
            {
                ss.Add(dt.ToString("yyy MMMM"));
                dt = dt.AddMonths(1);
            }
            return new { data = vv, name = ach.Name, labels = ss.ToArray() };
        }

        public object GetYearReport(int id)
        {
            AchieverContext context = AchieverContextHolder.GetContext();
            var user = Helper.GetUser(HttpContext.Session);

            var aa = context.AchievementValueItems.Include(z => z.User).Where(z =>
                 z.User.Id == user.Id && z.Achievement.Id == id).ToArray().GroupBy(z => z.Timestamp.Year).ToArray();


            List<int> vv = new List<int>();
            List<string> ss = new List<string>();

            foreach (var item in aa.OrderBy(z => z.Key))
            {
                ss.Add(item.Key.ToString());
                var sum = item.Sum(z => z.Count);
                vv.Add(sum);
            }

            var ach = context.AchievementItems.First(z => z.Id == id);            
            return new { data = vv, name = ach.Name, labels = ss.ToArray() };
        }

        [HttpGet("{id}/{monthId}/{yearId}/{chartType}")]
        public object Get(int id, int monthId, int yearId, string chartType)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            if (chartType == "day")
                return GetDayReport(monthId, yearId, id);
            if (chartType == "month")
                return GetMonthReport(yearId, id);
            if (chartType == "year")
                return GetYearReport(id);

            return BadRequest();

        }
    }
}
