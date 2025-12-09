using Achiever.Common.Model;
using System.Diagnostics;

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
                using var db = new AchieverContext();

                foreach (var item in db.Users.ToArray())
                {
                    Console.WriteLine(item.Login);
                }
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
