using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Achiever.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public string Login { get; set; }
        public string AvatarPath { get; set; }

        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public int PaidPeriod { get; set; }
        public bool GoldUser { get; set; }
        public bool Enabled { get; set; }
        public string XmlConfig { get; set; }
        public long? TelegramChatId { get; set; }
        public string GetXmlProp(string v)
        {
            if (string.IsNullOrEmpty(XmlConfig))            
                return null;                
            
            var doc = XDocument.Parse(XmlConfig);
            
            foreach (var item in doc.Descendants("param"))
            {
                var nm = item.Attribute("name").Value;
                var vl = item.Attribute("value").Value;
                if (nm == v)                
                    return vl;   
            }

            return null;
        }

        public void UpdateXmlProp(string v, string otp)
        {
            if(string.IsNullOrEmpty(XmlConfig))
            {
                XmlConfig = "<root></root>";
            }
            var doc = XDocument.Parse(XmlConfig);
            bool was = false;
            foreach (var item in doc.Descendants("param"))
            {
                var nm = item.Attribute("name").Value;
                var vl = item.Attribute("value").Value;
                if (nm == v)
                {
                    item.SetAttributeValue("value", otp);
                    was = true;
                    break;
                }
            }
            if (!was)
            {
                doc.Root.Add(new XElement("param", new XAttribute("name", v), new XAttribute("value", otp)));
            }
            XmlConfig = doc.ToString();

        }
    }
}
