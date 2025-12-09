using System;
using System.IO;
using System.Xml.Linq;

namespace Achiever.Telegram
{
    public static class ConfigLoader
    {
        public static string ConfigPath = "config.xml";
        public static bool? ReadBoolSetting(string name)
        {
            var raw = ReadSetting(name);
            return raw == null ? null : bool.Parse(raw);
        }

        public static string ReadSetting(string name)
        {
            if (!File.Exists(ConfigPath))
                return null;

            try
            {
                var doc = XDocument.Load(ConfigPath);
                foreach (var item in doc.Descendants("setting"))
                {
                    var nm = item.Attribute("name").Value;
                    string vl = null;
                    if (item.Attribute("value") != null)
                        vl = item.Attribute("value").Value;
                    else
                        vl = item.Value;
                    if (nm == name)
                        return vl;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}