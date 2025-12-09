using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Achiever;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using Achiever.Model;
using Achiever.Common.Model;

namespace Achiever
{
    public class Helper
    {
        

        

        public static decimal GetModifier(DateTime time)
        {
            AchieverContext ctx = new AchieverContext();
            decimal ret = 1;
            foreach (var item in ctx.Penalties.Include(z => z.Achievement))
            {
                var achId = item.Achievement.Id;
                var ww = ctx.AchievementValueItems.Where(z => z.Achievement.Id == achId).ToArray();
                var dd = ww.Where(z => time > z.Timestamp && Math.Abs((z.Timestamp - time).TotalDays) < item.Days).ToArray();
                for (int i = 0; i < dd.Length; i++)
                {
                    ret *= item.Modifier;
                }
            }

            return ret;
        }

        internal static bool IsAuthorized(ISession s)
        {
            return Helper.GetUser(s) != null;
        }

        public static bool IsComplete(int chId, int userId)
        {
            AchieverContext context = new AchieverContext();
            var user = context.Users.Find(userId);

            var userInfos = context.UserChallengeInfos.Where(z => z.ChallengeId == chId && userId == z.UserId).Include(z => z.Challenge).Include(z => z.Challenge.Aims).ToArray();
            if (!userInfos.Any()) 
                return false;

            if (userInfos.Any(z => z.IsComplete))
                return true;               
            
            var item = userInfos.Where(z => !z.IsComplete).First();
            
            foreach (var aim in item.Challenge.Aims)
            {
                bool compl = Helper.IsAimAchieved(item, aim, user);

                if (!compl)
                {
                    return false;
                }
            }

            var a1 = context.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == chId).ToArray();
            foreach (var zz in a1)
            {
                if (!IsComplete(zz.Child.Id, userId)) return false;
            }

            return true;

        }
        public static async void CheckAllChallenges(int userId)
        {
            AchieverContext context = new AchieverContext();
            var user = context.Users.Find(userId);
            foreach (var item in context.UserChallengeInfos.Where(z => !z.IsComplete && z.User.Id == userId).Include(z => z.Challenge).Include(z => z.Challenge.Aims))
            {
                if (IsComplete(item.ChallengeId, userId))
                {
                    item.IsComplete = true;
                    item.CompleteTime = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
        }

        public class AimLabels
        {
            public string Title;
        }

        public static AimLabels GetAimLabels(UserChallengeInfo chitem2, ChallengeAimItem bb, User user)
        {

            var ctx = new AchieverContext();
            var item = ctx.AchievementItems.SingleOrDefault(z => z.Id == bb.AchievementId);
            string clr = "#bbddff";
            bool achieved = Helper.IsAimAchieved(chitem2, bb, user);
            bool secondProgressBarEnabled = true;



            var chlds = ctx.AchievementItems.Where(z => z.Parent.Id == item.Id).Select(z => z.Id).ToArray();

            var aa = ctx.AchievementValueItems.Where(z => z.User.Id == user.Id
            && (z.Achievement.Id == item.Id || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > chitem2.StartTime.Value) || !chitem2.Challenge.UseValuesAfterStartOnly));

            /*
                        var aa = ctx.AchievementValueItems.Where(z => z.User.Id == user.Id &&
                        z.Achievement.Id == item.Id && ((z.Timestamp > chitem2.StartTime.Value)
                        || !chitem2.Challenge.UseValuesAfterStartOnly));*/

            //var bb = ctx.AchievementAimItems.FirstOrDefault(z => z.Achievement.Id == item.Id);


            if (bb == null)
            {
                throw new NotImplementedException();
                //<p>@item.Name: @cnt - цель не задана <a href="#">задать</a> </p>


            }

            int target = bb.Count.Value;


            var cnstr = Helper.ExtractAimSettings(bb);
            var percnt = Helper.GetPercentOfAim(bb, chitem2);
            AimLabels ret = new AimLabels();
            switch (bb.Type)
            {

                case AimType.Bool:
                    {
                        var addstr3 = "";
                        int cnt = 0;
                        int cnt11 = 0;

                        var addstr4 = "";
                        var todays = aa.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date);
                        if (cnstr != null)
                        {
                            switch (cnstr.Period)
                            {
                                case PeriodTypeEnum.Year:
                                    {

                                        addstr3 = "за год";
                                        if (aa.Any())
                                        {
                                            var fr = aa.ToList().GroupBy(z => z.Timestamp.Date.Year).OrderByDescending(z => z.Sum(u => u.Count)).First();


                                            addstr4 = fr.Key.ToString();
                                            cnt = fr.Sum(z => z.Count);
                                            todays = aa.Where(z => z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                            cnt11 = todays.Sum(z => z.Count);
                                        }
                                        else
                                        {

                                            addstr4 = "";
                                            cnt = 0;
                                            todays = aa.Where(z => z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                            cnt11 = todays.Sum(z => z.Count);
                                        }

                                    }
                                    break;
                                case PeriodTypeEnum.Month:
                                    {

                                        addstr3 = "за месяц";
                                        if (aa.Any())
                                        {
                                            var fr = aa.ToList().GroupBy(z => z.Timestamp.Date.Month + ";" + z.Timestamp.Date.Year).OrderByDescending(z => z.Sum(u => u.Count)).First();


                                            var split = fr.Key.Split(';').Select(int.Parse).ToArray();
                                            var dt = new DateTime(split[1], split[0], 1);

                                            addstr4 = dt.ToString("MMMM yyyy");
                                            cnt = fr.Sum(z => z.Count);
                                            todays = aa.Where(z => z.Timestamp.Date.Month == DateTime.UtcNow.Date.Month && z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                            cnt11 = todays.Sum(z => z.Count);
                                        }
                                        else
                                        {
                                            addstr4 = "";
                                            cnt = 0;
                                            todays = aa.Where(z => z.Timestamp.Date.Month == DateTime.UtcNow.Date.Month && z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                            cnt11 = todays.Sum(z => z.Count);
                                        }

                                    }
                                    break;
                                case PeriodTypeEnum.Day:
                                    {

                                        addstr3 = "за день";

                                        if (cnstr.MinCountPerSet != null)
                                        {
                                            var lst = aa.Where(z => z.Count >= cnstr.MinCountPerSet.Value).ToList();
                                            if (lst.Any())
                                            {
                                                var fr = lst.GroupBy(z => z.Timestamp.Date).OrderByDescending(z => z.Sum(u => u.Count)).First();
                                                var todays1 = aa.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).Where(z => z.Count >= cnstr.MinCountPerSet.Value);
                                                var lastOne = fr.OrderByDescending(z => z.Timestamp).First();
                                                addstr4 = lastOne.Timestamp.ToLongDateString() + " " + lastOne.Timestamp.ToLongTimeString();

                                                cnt = fr.Sum(z => z.Count);
                                                if (todays1.Any())
                                                {
                                                    cnt11 = todays1.Sum(z => z.Count);
                                                }
                                            }
                                            else
                                            {

                                            }
                                            addstr3 += $" (мин. {cnstr.MinCountPerSet.Value} за подход)";
                                        }
                                        else
                                        {
                                            if (aa.Any())
                                            {
                                                var fr = aa.ToList().GroupBy(z => z.Timestamp.Date).OrderByDescending(z => z.Sum(u => u.Count)).First();

                                                addstr4 = fr.Key.ToLongDateString();
                                                cnt = fr.Sum(z => z.Count);
                                                cnt11 = todays.Sum(z => z.Count);
                                            }
                                            else
                                            {

                                                addstr4 = "";
                                                cnt = 0;
                                                cnt11 = todays.Sum(z => z.Count);
                                            }
                                        }
                                    }
                                    break;
                                case PeriodTypeEnum.Set:
                                    {
                                        addstr3 = "за подход";
                                        if (cnstr.Times != null)
                                        {
                                            addstr3 = $"за подход ({bb.Count.Value} повторений)";
                                            target = cnstr.Times.Value;
                                            secondProgressBarEnabled = false;
                                            var lst = aa.Where(z => z.Count >= bb.Count).ToList();
                                            cnt = lst.Count;
                                            addstr3 += $" (требуется подходов за все время: {cnstr.Times.Value})";
                                        }
                                        else
                                        if (aa.Any())
                                        {
                                            var fr = aa.OrderByDescending(z => z.Count).First();


                                            addstr4 = fr.Timestamp.ToLongDateString() + " " + fr.Timestamp.ToLongTimeString();
                                            cnt = fr.Count;
                                            if (todays.Any())
                                            {
                                                cnt11 = todays.Max(z => z.Count);
                                            }
                                        }
                                        else
                                        {
                                            addstr4 = "";
                                            cnt = 0;
                                            if (todays.Any())
                                            {
                                                cnt11 = todays.Max(z => z.Count);
                                            }
                                        }
                                    }

                                    break;
                                case PeriodTypeEnum.Whole:
                                    addstr3 = "за все время";
                                    if (aa.Any())
                                    {
                                        cnt = aa.Sum(z => z.Count);
                                        cnt11 = aa.Sum(z => z.Count);
                                    }
                                    else
                                    {
                                        cnt = 0;
                                        cnt11 = 0;
                                    }
                                    break;
                            }
                        }




                        bool done = false;
                        if (cnt > target)
                        {
                            cnt = target;
                            done = true;

                        }
                        var percent = cnt / (float)target;
                        var percent11 = cnt11 / (float)target;
                        var percent2 = Math.Round(percent * 100, 0);
                        var percent22 = Math.Round(percent11 * 100, 0);

                        var res = $"{item.Name} {addstr3}: {cnt} / {target}({percent2} %)  {addstr4}";

                        ret.Title = res;

                    }
                    break;
                case AimType.Count:
                    {

                        int cnt = aa.Sum(z => z.Count);

                        if (cnt > target)
                        {
                            cnt = target;
                        }
                        var percent = cnt / (float)target;
                        var percent2 = Math.Round(percent * 100, 0);
                        var addstr2 = "";

                        var res = $"{item.Name}: {cnt} / {target}({percent2} %)  {addstr2}";

                        ret.Title = res;

                    }
                    break;
                case AimType.Min:
                    {

                        ret.Title = $"{item.Name}:  {target}(min)";

                    }
                    break;
                case AimType.Max:
                    {

                        ret.Title = $"{item.Name}:  {target}(max)";
                    }
                    break;
            }
            return ret;
        }
        public static bool IsAimAchieved(UserChallengeInfo info, ChallengeAimItem aim, User user)
        {
            var context = new AchieverContext();
            //var aa = context.AchievementValueItems.Where(z => z.User.Id == user.Id && aim.AchievementId == z.Achievement.Id);

            var chlds = context.AchievementItems.Where(z => z.Parent.Id == aim.AchievementId).Select(z => z.Id).ToArray();

            var aa = context.AchievementValueItems.Where(z => z.User.Id == user.Id
            && (z.Achievement.Id == aim.AchievementId || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly));
            if (aa.Count() == 0)
            {
                return false;
            }

            switch (aim.Type)
            {
                case AimType.Count:
                    var cnt = aa.Sum(z => z.Count);
                    var target = aim.Count;
                    if (string.IsNullOrEmpty(aim.Settings))
                    {
                        if (cnt >= target) return true;
                    }
                    break;
                case AimType.Days:
                    break;
                case AimType.Bool:
                    {
                        var cnstr = Helper.ExtractAimSettings(aim);
                        if (cnstr != null)
                        {
                            switch (cnstr.Period)
                            {
                                case PeriodTypeEnum.Year:
                                    {

                                        var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                       info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == aim.AchievementId).
                                       ToArray().GroupBy(z => z.Timestamp.Date.Year).ToArray();
                                        if (ww1.Any(z => z.Sum(u => u.Count) >= aim.Count))
                                        {
                                            return true;
                                        }
                                    }
                                    break;
                                case PeriodTypeEnum.Month:
                                    {

                                        var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                       info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == aim.AchievementId).
                                       ToArray().GroupBy(z => z.Timestamp.Date.Month + ";" + z.Timestamp.Date.Year).ToArray();
                                        if (ww1.Any(z => z.Sum(u => u.Count * GetModifier(u.Timestamp)) >= aim.Count))
                                            return true;
                                    }
                                    break;
                                case PeriodTypeEnum.Day:
                                    {

                                        var ww0 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                       info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == aim.AchievementId).ToArray();
                                        if (cnstr.MinCountPerSet != null)
                                        {
                                            ww0 = ww0.Where(z => z.Count >= cnstr.MinCountPerSet).ToArray();
                                        }
                                        var ww1 = ww0.ToArray().GroupBy(z => z.Timestamp.Date).ToArray();
                                        int times = 1;
                                        if (cnstr.Times != null)
                                        {
                                            times = cnstr.Times.Value;

                                        }
                                        return ww1.Count(z => z.Sum(u => u.Count) >= aim.Count) >= times;
                                    }
                                    break;
                                case PeriodTypeEnum.Set:
                                    {
                                        var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                        info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == aim.AchievementId);
                                        if (ww1.Any())
                                        {
                                            var fr = ww1.OrderByDescending(z => z.Count).First();
                                            if (cnstr.Times != null)
                                            {
                                                return ww1.Count(z => z.Count >= aim.Count) >= cnstr.Times;
                                            }
                                            if (fr.Count >= aim.Count)
                                            {
                                                return true;

                                            }
                                        }
                                    }

                                    break;
                                case PeriodTypeEnum.Whole:
                                    { }

                                    break;
                            }
                        }
                    }
                    break;
                case AimType.Max:
                    break;
                case AimType.Min:
                    break;
                default:
                    break;
            }
            return false;
        }

        public static double GetProgressLastDay(UserChallengeInfo chitem2)
        {
            var nw = DateTime.UtcNow.Date;

            var p = GetPercentOfChallenge(chitem2.ChallengeId, chitem2.UserId, nw);
            var p2 = GetPercentOfChallenge(chitem2.ChallengeId, chitem2.UserId);

            return p2 - p;
        }

        public static double GetProgressLastDay(ChallengeAimItem aim, UserChallengeInfo uci)
        {
            var nw = DateTime.UtcNow.Date;

            var p = GetPercentOfAim(aim, uci, nw);
            var p2 = GetPercentOfAim(aim, uci);
            return p2 - p;
        }

        public static string[] GetText(string json)
        {
            dynamic stuff = JsonConvert.DeserializeObject(json);


            List<string> sss = new List<string>();
            foreach (var item in stuff.text)
            {
                var s1 = item.ToString();
                sss.Add(s1);
            }

            return sss.ToArray();

        }

        public static User GetUser(ISession s)
        {
            return s.GetObject<User>("user");
        }

        public static double GetPercentOfChallenge(int chId, int userId, DateTime? lastFilter = null)
        {
            var ctx = new AchieverContext();
            if (!ctx.UserChallengeInfos.Any(z => z.UserId == userId && z.ChallengeId == chId))
                return 0;

            UserChallengeInfo chitem2 = ctx.UserChallengeInfos.Include(z => z.Challenge).Include(z => z.Challenge.Aims).Include(z => z.User).FirstOrDefault(z => z.UserId == userId && z.ChallengeId == chId);
            double perctot = 0;

            foreach (var item in chitem2.Challenge.Aims)
            {
                perctot += Helper.GetPercentOfAim(item, chitem2, lastFilter);
            }
            var ar1 = ctx.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == chitem2.ChallengeId).ToArray();
            foreach (var item in ar1)
            {
                var fr = ctx.UserChallengeInfos.FirstOrDefault(z => z.ChallengeId == item.Child.Id && z.UserId == chitem2.UserId);
                if (fr == null)
                    continue;

                perctot += GetPercentOfChallenge(fr.ChallengeId, fr.UserId, lastFilter);
            }

            perctot /= (chitem2.Challenge.Aims.Count + ar1.Length);
            //todo: calc all required challenges

            return perctot;
        }

        public static double GetPercentOfAim(ChallengeAimItem bb, UserChallengeInfo chitem2, DateTime? lastFilter = null)
        {
            int target = bb.Count.Value;
            var ctx = new AchieverContext();

            /*var aa = ctx.AchievementValueItems.Where(z =>
            z.User.Id == chitem2.UserId &&
            z.Achievement.Id == bb.AchievementId && ((z.Timestamp > chitem2.StartTime.Value) || !chitem2.Challenge.UseValuesAfterStartOnly));
            */

            var chlds = ctx.AchievementItems.Where(z => z.Parent.Id == bb.AchievementId).Select(z => z.Id).ToArray();

            var aa = ctx.AchievementValueItems.Where(z => z.User.Id == chitem2.UserId
            && (z.Achievement.Id == bb.AchievementId || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > chitem2.StartTime.Value) || !chitem2.Challenge.UseValuesAfterStartOnly));

            if (lastFilter != null)
            {
                aa = aa.Where(z => z.Timestamp < lastFilter.Value);
            }

            var cnstr = Helper.ExtractAimSettings(bb);
            if (aa.Count() == 0)
            {
                return 0;
            }

            switch (bb.Type)
            {
                case AimType.Bool:
                    {
                        int cnt = 0;
                        switch (cnstr.Period)
                        {
                            case PeriodTypeEnum.Month:
                                {
                                    var fr = aa.ToList().GroupBy(z => z.Timestamp.Date.Month + ";" + z.Timestamp.Date.Year).OrderByDescending(z => z.Sum(u => u.Count)).First();
                                    cnt = fr.Sum(z => z.Count);
                                }
                                break;
                            case PeriodTypeEnum.Day:
                                {
                                    var aaa = aa.ToList();
                                    if (cnstr.MinCountPerSet != null)
                                    {
                                        aaa = aaa.Where(z => z.Count >= cnstr.MinCountPerSet.Value).ToList();
                                    }
                                    if (aaa.Count == 0)
                                        break;

                                    var fr = aaa.GroupBy(z => z.Timestamp.Date).OrderByDescending(z => z.Sum(u => u.Count)).First();


                                    if (cnstr.Times != null)
                                    {
                                        cnt = aaa.GroupBy(z => z.Timestamp.Date).Where(z => z.Sum(u => u.Count) >= bb.Count.Value).Count();
                                        target = cnstr.Times.Value;
                                    }
                                    else
                                    {
                                        cnt = fr.Sum(z => z.Count);
                                    }

                                }
                                break;
                            case PeriodTypeEnum.Set:
                                {
                                    var fr = aa.OrderByDescending(z => z.Count).First();
                                    cnt = fr.Count;

                                    if (cnstr.Times != null)
                                    {

                                        target = cnstr.Times.Value;

                                        var lst = aa.Where(z => z.Count >= bb.Count).ToList();
                                        cnt = lst.Count;

                                    }
                                    else
                                    if (aa.Any())
                                    {
                                        var fr2 = aa.OrderByDescending(z => z.Count).First();
                                        cnt = fr2.Count;
                                    }
                                    else
                                    {
                                        cnt = 0;
                                    }
                                }
                                break;
                            case PeriodTypeEnum.Whole:
                                { cnt = aa.Sum(z => z.Count); }
                                break;
                        }

                        if (cnt > target)
                        {
                            cnt = target;
                        }
                        return cnt / (float)target;
                    }
                    break;
                case AimType.Count:
                    {
                        var cnt = aa.Sum(z => z.Count);
                        if (cnt > target)
                        {
                            cnt = target;
                        }
                        var percent = cnt / (float)target;
                        return percent;
                    }
                    break;
            }
            return 0;
        }

        public static AimConstraints ExtractAimSettings(ChallengeAimItem aim)
        {
            AimConstraints ret = new AimConstraints();
            if (string.IsNullOrEmpty(aim.Settings)) return null;
            dynamic stuff = JsonConvert.DeserializeObject(aim.Settings);

            var vals = Enum.GetValues(typeof(PeriodTypeEnum));
            foreach (var va in vals)
            {
                if (stuff.set != null && stuff.set.constraints != null && stuff.set.constraints.minCount != null)
                {
                    ret.MinCountPerSet = int.Parse(stuff.set.constraints.minCount.ToString());
                }
                if (stuff.times != null)
                {
                    ret.Times = int.Parse(stuff.times.ToString());
                }
                if (stuff.set != null && stuff.set.constraints != null && stuff.set.constraints.amount != null)
                {
                    ret.Times = int.Parse(stuff.set.constraints.amount.ToString());
                }
                if (va.ToString().ToLower() == stuff.period.ToString())
                {
                    ret.Period = (PeriodTypeEnum)va;
                }
            }

            return ret;
        }

        public static int? GetFontSize(string json)
        {
            dynamic stuff = JsonConvert.DeserializeObject(json);
            if (stuff.fontSize != null)
            {
                return int.Parse(stuff.fontSize.ToString());
            }
            return null;

        }
        public static string GetHardness(string json)
        {
            dynamic stuff = JsonConvert.DeserializeObject(json);
            if (stuff.hardness != null)
            {
                return (stuff.hardness.ToString());
            }
            return "-";

        }

        public static string? GetBackColor(string json)
        {
            try
            {
                dynamic stuff = JsonConvert.DeserializeObject(json);
                if (stuff.backColor != null)
                {
                    return stuff.backColor.ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static string? GetColor(string json)
        {
            try
            {
                dynamic stuff = JsonConvert.DeserializeObject(json);
                if (stuff.color != null)
                {
                    return stuff.color.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
