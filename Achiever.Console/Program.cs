using Achiever.Common.Model;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Xml.Linq;

namespace Achiever
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Achiever console");
            var parsedArgs = args.Select(ParseArg).ToArray();

            if (parsedArgs.Any(z => z.Item1.StartsWith("--debugger")))
                Debugger.Launch();

            string action = "";


            if (parsedArgs.Any(z => z.Item1 == "--action"))
                action = parsedArgs.First(z => z.Item1 == "--action").Item2;

            if (action == "showUsersList")
            {
                using var db = AchieverContextHolder.GetContext();

                foreach (var item in db.Users.ToArray())
                {
                    Console.WriteLine(item.Login);
                }
            }
            else if (action == "importDbFromXml")
            {
                string folderPath = "";

                if (parsedArgs.Any(z => z.Item1 == "--folderPath"))
                    folderPath = parsedArgs.First(z => z.Item1 == "--folderPath").Item2;

                using var db = AchieverContextHolder.GetContext();
                db.Database.EnsureDeleted();
                db.SaveChanges();

                db.Database.EnsureCreated();
                var dinf = new DirectoryInfo(folderPath);
                foreach (var item in dinf.GetFiles("*.xml"))
                {
                    var doc = XDocument.Load(item.FullName);
                    if (item.Name.Contains("Users"))
                    {
                        foreach (var uitem in db.Users)
                        {

                        }
                        foreach (var el in doc.Descendants("DATA_RECORD"))
                        {
                            var ret = db.Users.Add(new Model.User()
                            {
                                //Id = int.Parse(el.Element("Id").Value),
                                Name = el.Element("Name").Value,
                                AvatarPath = el.Element("AvatarPath").Value,
                                Login = el.Element("Login").Value,
                                Password = el.Element("Password").Value,
                                TelegramChatId = !string.IsNullOrEmpty(el.Element("TelegramChatId").Value) ? long.Parse(el.Element("TelegramChatId").Value.Replace(",", "")) : null,
                                Enabled = el.Element("Enabled").Value == "1",
                                IsAdmin = el.Element("IsAdmin").Value == "1",
                                PaidPeriod = int.Parse(el.Element("PaidPeriod").Value),
                                GoldUser = el.Element("GoldUser").Value == "1"
                            });
                            ret.Entity.Id = int.Parse(el.Element("Id").Value);
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        static (string, string) ParseArg(string str)
        {
            var spl = str.Split("=");
            if (spl.Length > 1)
            {
                var ret = spl[1];
                return (spl[0], ret);
            }
            return (spl[0], string.Empty);
        }

    }
}
