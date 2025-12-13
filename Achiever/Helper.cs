using Achiever;
using Achiever.Common.Model;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Achiever
{
    public partial class Helper
    {

      
        internal static bool IsAuthorized(ISession s)
        {
            return Helper.GetUser(s) != null;
        }

     
        public static async void CheckAllChallenges(int userId)
        {
            AchieverContext context = AchieverContextHolder.GetContext();
            var user = context.Users.Find(userId);
            foreach (var item in context.UserChallengeInfos.Where(z => !z.IsComplete && z.User.Id == userId).Include(z => z.Challenge).Include(z => z.Challenge.Aims))
            {
                item.CheckAndUpdateComplete(context);
                //if (IsComplete(item.ChallengeId, userId))
                //{
                //    item.IsComplete = true;
                //    item.CompleteTime = DateTime.Now;
                //    await context.SaveChangesAsync();
                //}
            }
        }



        public static AimLabels GetAimLabels(AchieverContext ctx, UserChallengeInfo chitem2, ChallengeAimItem bb, User user)
        {
            var item = ctx.AchievementItems.SingleOrDefault(z => z.Id == bb.AchievementId);
            string clr = "#bbddff";
            bool achieved = bb.IsAimAchieved(ctx, chitem2, user);
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

            var cnstr = bb.ExtractAimSettings();
            var percnt = bb.GetPercentOfAim(ctx, chitem2);
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



        public static double GetProgressLastDay(AchieverContext ctx, UserChallengeInfo chitem2)
        {
            var nw = DateTime.UtcNow.Date;

            var p = GetPercentOfChallenge(ctx, chitem2.ChallengeId, chitem2.UserId, nw);
            var p2 = GetPercentOfChallenge(ctx, chitem2.ChallengeId, chitem2.UserId);

            return p2 - p;
        }

        public static double GetProgressLastDay(AchieverContext ctx, ChallengeAimItem aim, UserChallengeInfo uci)
        {
            var nw = DateTime.UtcNow.Date;

            var p = aim.GetPercentOfAim(ctx, uci, nw);
            var p2 = aim.GetPercentOfAim(ctx, uci);
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

        public static double GetPercentOfChallenge(AchieverContext ctx, int chId, int userId, DateTime? lastFilter = null)
        {
            if (!ctx.UserChallengeInfos.Any(z => z.UserId == userId && z.ChallengeId == chId))
                return 0;

            UserChallengeInfo chitem2 = ctx.UserChallengeInfos.Include(z => z.Challenge).Include(z => z.Challenge.Aims).Include(z => z.User).FirstOrDefault(z => z.UserId == userId && z.ChallengeId == chId);
            double perctot = 0;

            foreach (var item in chitem2.Challenge.Aims)
            {
                perctot += item.GetPercentOfAim(ctx, chitem2, lastFilter);
            }
            var ar1 = ctx.ChallengeRequirements.Include(z => z.Parent).Include(z => z.Child).Where(z => z.Parent.Id == chitem2.ChallengeId).ToArray();
            foreach (var item in ar1)
            {
                var fr = ctx.UserChallengeInfos.FirstOrDefault(z => z.ChallengeId == item.Child.Id && z.UserId == chitem2.UserId);
                if (fr == null)
                    continue;

                perctot += GetPercentOfChallenge(ctx, fr.ChallengeId, fr.UserId, lastFilter);
            }

            perctot /= (chitem2.Challenge.Aims.Count + ar1.Length);
            //todo: calc all required challenges

            return perctot;
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
