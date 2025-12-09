using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Achiever.Model
{
    public class AchievementItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public string Settings { get; set; }

        public int? OwnerId { get; set; }
        public User Owner { get; set; }
        public AchievementItem Parent { get; set; }
        public Features GetFeatures( )
        {
            Features ret = new Features();
            if (string.IsNullOrEmpty(Settings)) 
                return null;

            dynamic stuff = JsonConvert.DeserializeObject(Settings);


            if (stuff.cumulative != null)
            {
                ret.IsCumulative = int.Parse(stuff.cumulative.ToString()) == 1;
            }
            if (stuff.features != null)
            {
                foreach (var item2 in stuff.features)
                {
                    if (item2.ToString() == "singular")
                    {
                        ret.IsSingular = true;
                    }
                    if (item2.ToString() == "doubleValued")
                    {
                        ret.IsDoubleValued = true;
                    }
                }
            }


            return ret;
        }
    }
}
