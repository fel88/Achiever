using Achiever.Common.Model;
using Newtonsoft.Json;
using System;

namespace Achiever.Model
{
    public class ChallengeAimItem
    {
        public int Id { get; set; }
        public int AchievementId { get; set; }
        public AchievementItem Achievement { get; set; }

        public DateTime? UntilDate { get; set; }
        public int? Count { get; set; }//target count
        public int? DaysPeriod { get; set; }
        public int? MinPerDayCount { get; set; }
        public int? MaxDaysGap { get; set; }

        public AimType Type { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// json settings
        /// </summary>
        public string Settings { get; set; }

        public bool IsAimAchieved(AchieverContext context, UserChallengeInfo info, User user)
        {
            //var aa = context.AchievementValueItems.Where(z => z.User.Id == user.Id && aim.AchievementId == z.Achievement.Id);

            var chlds = context.AchievementItems.Where(z => z.Parent.Id == AchievementId).Select(z => z.Id).ToArray();

            var aa = context.AchievementValueItems.Where(z => z.User.Id == user.Id
            && (z.Achievement.Id == AchievementId || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly));
            if (aa.Count() == 0)
            {
                return false;
            }

            switch (Type)
            {
                case AimType.Count:
                    var cnt = aa.Sum(z => z.Count);
                    var target = Count;
                    if (string.IsNullOrEmpty(Settings))
                    {
                        if (cnt >= target) return true;
                    }
                    break;
                case AimType.Days:
                    break;
                case AimType.Bool:
                    {
                        var cnstr = ExtractAimSettings();
                        if (cnstr == null)                        
                            break;
                        
                        switch (cnstr.Period)
                        {
                            case PeriodTypeEnum.Year:
                                {

                                    var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                   info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == AchievementId).
                                   ToArray().GroupBy(z => z.Timestamp.Date.Year).ToArray();
                                    if (ww1.Any(z => z.Sum(u => u.Count) >= Count))
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case PeriodTypeEnum.Month:
                                {

                                    var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                   info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == AchievementId).
                                   ToArray().GroupBy(z => z.Timestamp.Date.Month + ";" + z.Timestamp.Date.Year).ToArray();
                                    if (ww1.Any(z => z.Sum(u => u.Count * context.GetModifier(u.Timestamp)) >= Count))
                                        return true;
                                }
                                break;
                            case PeriodTypeEnum.Day:
                                {

                                    var ww0 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                   info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == AchievementId).ToArray();
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
                                    return ww1.Count(z => z.Sum(u => u.Count) >= Count) >= times;
                                }
                                break;
                            case PeriodTypeEnum.Set:
                                {
                                    var ww1 = context.AchievementValueItems.Where(z => z.User.Id == user.Id && ((z.Timestamp >
                                    info.StartTime.Value) || !info.Challenge.UseValuesAfterStartOnly) && z.Achievement.Id == AchievementId);
                                    if (ww1.Any())
                                    {
                                        var fr = ww1.OrderByDescending(z => z.Count).First();
                                        if (cnstr.Times != null)
                                        {
                                            return ww1.Count(z => z.Count >= Count) >= cnstr.Times;
                                        }
                                        if (fr.Count >= Count)
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
                        break;
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
        public  AimStrings ExtractStringsForBoolAim( AchieverContext ctx, int itemId, UserChallengeInfo chitem2)
        {
            int userId = chitem2.UserId;
            AimStrings ret = new AimStrings();
            var chlds = ctx.AchievementItems.Where(z => z.Parent.Id == itemId).Select(z => z.Id).ToArray();

            var aa = ctx.AchievementValueItems.Where(z => z.User.Id == userId &&
            (z.Achievement.Id == itemId || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > chitem2.StartTime.Value)
            || !chitem2.Challenge.UseValuesAfterStartOnly));
            var cnstr = ExtractAimSettings();



            var todays = aa.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date);
            ret.target = Count.Value;
            int target2 = Count.Value;



            if (cnstr != null)
            {
                switch (cnstr.Period)
                {
                    case PeriodTypeEnum.Year:
                        {

                            ret.addstr3 = "за год";
                            if (aa.Any())
                            {
                                var fr = aa.ToList().GroupBy(z => z.Timestamp.Date.Year).OrderByDescending(z => z.Sum(u => u.Count)).First();


                                ret.addstr4 = fr.Key.ToString();
                                ret.cnt = fr.Sum(z => z.Count);
                                todays = aa.Where(z => z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                ret.cnt11 = todays.Sum(z => z.Count);
                            }
                            else
                            {

                                ret.addstr4 = "";
                                ret.cnt = 0;
                                todays = aa.Where(z => z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                ret.cnt11 = todays.Sum(z => z.Count);
                            }

                        }
                        break;
                    case PeriodTypeEnum.Month:
                        {
                            ret.addstr3 = "за месяц";
                            if (aa.Any())
                            {
                                var fr = aa.ToList().GroupBy(z => $"{z.Timestamp.Date.Month};{z.Timestamp.Date.Year}").OrderByDescending(z => z.Sum(u => u.Count * ctx.GetModifier(u.Timestamp))).First();

                                var split = fr.Key.Split(';').Select(int.Parse).ToArray();
                                var dt = new DateTime(split[1], split[0], 1);

                                ret.addstr4 = dt.ToString("MMMM yyyy");
                                ret.cnt = (int)fr.Sum(z => z.Count * ctx.GetModifier(z.Timestamp));
                                todays = aa.Where(z => z.Timestamp.Date.Month == DateTime.UtcNow.Date.Month && z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                var rawCount = todays.Sum(z => z.Count);
                                ret.cnt11 = (int)todays.ToArray().Sum(z => z.Count * ctx.GetModifier(z.Timestamp));
                                var required = (int)Math.Ceiling(target2 / (float)DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));
                                var diff = ret.cnt11 - DateTime.UtcNow.Day * required;
                                var penalty = rawCount - ret.cnt11;
                                if (diff >= 0)
                                {
                                    ret.addstr5 = $"опрежение +{diff}";
                                }
                                else
                                {
                                    ret.addstr5 = $"отставание {diff}";
                                }
                                if (penalty > 0)
                                {
                                    ret.addstr5 += $" (штраф {penalty}) модификатор: x{ctx.GetModifier(DateTime.UtcNow)}";
                                }
                            }
                            else
                            {
                                ret.addstr4 = "";
                                ret.cnt = 0;
                                todays = aa.Where(z => z.Timestamp.Date.Month == DateTime.UtcNow.Date.Month && z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
                                ret.cnt11 = todays.Sum(z => z.Count);
                            }

                        }
                        break;
                    case PeriodTypeEnum.Day:
                        {
                            ret.addstr3 = "за день";
                            if (cnstr.Times != null)
                            {
                                ret.addstr3 = $"за день ({Count.Value} повторений)";
                                var lst = aa.ToList();
                                if (cnstr.MinCountPerSet != null)
                                {
                                    lst = aa.Where(z => z.Count >= cnstr.MinCountPerSet.Value).ToList();
                                    ret.addstr3 += $" (мин. {cnstr.MinCountPerSet.Value} за подход)";
                                }
                                ret.cnt = lst.GroupBy(z => z.Timestamp.Date).Where(z => z.Sum(u => u.Count) >= ret.target).Count();
                                ret.target = cnstr.Times.Value;
                                var todays1 = aa.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).Where(z => z.Count >= cnstr.MinCountPerSet.Value);
                                if (todays1.Any())
                                {
                                    ret.cnt11 = todays1.Sum(z => z.Count);
                                }


                                /*addstr3 = $"за подход ({bb.Count.Value} повторений)";
                                target = cnstr.Times.Value;
                                secondProgressBarEnabled = false;
                                var lst = aa.Where(z => z.Count >= bb.Count).ToList();
                                cnt = lst.Count;*/
                                ret.addstr3 += $" (требуется дней за все время: {cnstr.Times.Value})";
                            }
                            else if (cnstr.MinCountPerSet != null)
                            {
                                var lst = aa.Where(z => z.Count >= cnstr.MinCountPerSet.Value).ToList();
                                if (lst.Any())
                                {
                                    var fr = lst.GroupBy(z => z.Timestamp.Date).OrderByDescending(z => z.Sum(u => u.Count)).First();
                                    var todays1 = aa.Where(z => z.Timestamp.Date == DateTime.UtcNow.Date).Where(z => z.Count >= cnstr.MinCountPerSet.Value);
                                    var lastOne = fr.OrderByDescending(z => z.Timestamp).First();
                                    ret.addstr4 = lastOne.Timestamp.ToLongDateString() + " " + lastOne.Timestamp.ToLongTimeString();

                                    ret.cnt = fr.Sum(z => z.Count);
                                    if (todays1.Any())
                                    {
                                        ret.cnt11 = todays1.Sum(z => z.Count);
                                    }
                                }
                                else
                                {

                                }
                                ret.addstr3 += $" (мин. {cnstr.MinCountPerSet.Value} за подход)";
                            }
                            else
                            {
                                if (aa.Any())
                                {
                                    var fr = aa.ToList().GroupBy(z => z.Timestamp.Date).OrderByDescending(z => z.Sum(u => u.Count)).First();

                                    ret.addstr4 = fr.Key.ToLongDateString();
                                    ret.cnt = fr.Sum(z => z.Count);
                                    ret.cnt11 = todays.Sum(z => z.Count);
                                }
                                else
                                {

                                    ret.addstr4 = "";
                                    ret.cnt = 0;
                                    ret.cnt11 = todays.Sum(z => z.Count);
                                }
                            }
                        }
                        break;
                    case PeriodTypeEnum.Set:
                        {
                            ret.addstr3 = "за подход";
                            if (cnstr.Times != null)
                            {
                                ret.addstr3 = $"за подход ({Count.Value} повторений)";
                                ret.target = cnstr.Times.Value;
                                ret.secondProgressBarEnabled = false;
                                var lst = aa.Where(z => z.Count >= Count).ToList();
                                ret.cnt = lst.Count;
                                ret.addstr3 += $" (требуется подходов за все время: {cnstr.Times.Value})";
                            }
                            else
                            if (aa.Any())
                            {
                                var fr = aa.OrderByDescending(z => z.Count).First();


                                ret.addstr4 = fr.Timestamp.ToLongDateString() + " " + fr.Timestamp.ToLongTimeString();
                                ret.cnt = fr.Count;
                                if (todays.Any())
                                {
                                    ret.cnt11 = todays.Max(z => z.Count);
                                }
                            }
                            else
                            {
                                ret.addstr4 = "";
                                ret.cnt = 0;
                                if (todays.Any())
                                {
                                    ret.cnt11 = todays.Max(z => z.Count);
                                }
                            }
                        }

                        break;
                    case PeriodTypeEnum.Whole:
                        ret.addstr3 = "за все время";
                        if (aa.Any())
                        {
                            ret.cnt = aa.Sum(z => z.Count);
                            ret.cnt11 = aa.Sum(z => z.Count);
                        }
                        else
                        {
                            ret.cnt = 0;
                            ret.cnt11 = 0;
                        }
                        break;
                }
            }
            return ret;
        }

        public double GetPercentOfAim(AchieverContext ctx, UserChallengeInfo chitem2, DateTime? lastFilter = null)
        {
            int target = Count.Value;

            /*var aa = ctx.AchievementValueItems.Where(z =>
            z.User.Id == chitem2.UserId &&
            z.Achievement.Id == bb.AchievementId && ((z.Timestamp > chitem2.StartTime.Value) || !chitem2.Challenge.UseValuesAfterStartOnly));
            */

            var chlds = ctx.AchievementItems.Where(z => z.Parent.Id == AchievementId).Select(z => z.Id).ToArray();

            var aa = ctx.AchievementValueItems.Where(z => z.User.Id == chitem2.UserId
            && (z.Achievement.Id == AchievementId || chlds.Contains(z.Achievement.Id)) && ((z.Timestamp > chitem2.StartTime.Value) || !chitem2.Challenge.UseValuesAfterStartOnly));

            if (lastFilter != null)
            {
                aa = aa.Where(z => z.Timestamp < lastFilter.Value);
            }

            var cnstr = ExtractAimSettings();
            if (aa.Count() == 0)
                return 0;

            switch (Type)
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
                                        cnt = aaa.GroupBy(z => z.Timestamp.Date).Where(z => z.Sum(u => u.Count) >= Count.Value).Count();
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

                                        var lst = aa.Where(z => z.Count >= Count).ToList();
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

        public AimConstraints ExtractAimSettings()
        {
            AimConstraints ret = new AimConstraints();
            if (string.IsNullOrEmpty(Settings))
                return null;

            dynamic stuff = JsonConvert.DeserializeObject(Settings);

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
    }
}
