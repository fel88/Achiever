using Achiever.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Achiever.Common.Model
{
    public class AchieverContext : DbContext
    {
        public AchieverContext()
        {
            Database.EnsureCreated();
            if (!Users.Any() || !Users.Any(z => z.Login == "local_admin"))
            {
                Users.Add(new User() { Enabled = true, IsAdmin = true, Login = "local_admin", Name = "local_admin", Password = "12345".ComputeSha256Hash() });
                SaveChanges();
            }
        }
        public string GetDatabaseFilePath()
        {
            // Get the current database connection
            var connection = Database.GetDbConnection();

            // If the connection is a SqliteConnection, extract the DataSource
            if (connection is SqliteConnection sqliteConnection)
            {
                // The DataSource property contains the path from the connection string
                string dataSource = sqliteConnection.DataSource;

                // SQLite treats paths relative to the current working directory. 
                // To get the absolute path, combine it with the application base directory if it's relative.
                if (!Path.IsPathRooted(dataSource) && !dataSource.StartsWith("|DataDirectory|"))
                {
                    // This logic helps resolve the actual path at runtime
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    return Path.GetFullPath(Path.Combine(basePath, dataSource));
                }
                else if (dataSource.StartsWith("|DataDirectory|"))
                {
                    // Handle |DataDirectory| substitution string
                    string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                    if (!string.IsNullOrEmpty(dataDirectory))
                    {
                        return Path.GetFullPath(Path.Combine(dataDirectory, dataSource.Replace("|DataDirectory|", "")));
                    }
                }

                return dataSource;
            }

            return "Not a SQLite connection or path not found.";
        }
        public DbSet<User> Users { get; set; }
        public DbSet<AchievementItem> AchievementItems { get; set; }
        public DbSet<AchievementValueItem> AchievementValueItems { get; set; }
        public DbSet<DoubleAchievementValueItem> DoubleAchievementValueItems { get; set; }
        public DbSet<ChallengeAimItem> ChallengeAimItems { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallengeInfo> UserChallengeInfos { get; set; }
        public DbSet<ChallengeRequirement> ChallengeRequirements { get; set; }
        public DbSet<PenaltyItem> Penalties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   // ... other model configurations

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbProvider = ConfigLoader.ReadSetting("dbProvider");
           // var connectionString = ConfigLoader.ReadSetting("dbConnectionString");
            //Console.WriteLine("dbProvider: " + dbProvider);
          //  Console.WriteLine("connectionString: " + connectionString);
           // if (dbProvider == "postgres")
            {
            //    options.UseNpgsql(connectionString);
            }
           // else
            {
                // options.UseSqlite(connectionString ?? "Data Source=achiever.db");
                 options.UseSqlite("Data Source=achiever.db");
            }

        }
        public  decimal GetModifier(DateTime time)
        {
            decimal ret = 1;
            foreach (var item in Penalties.Include(z => z.Achievement).ToArray())
            {
                var achId = item.Achievement.Id;
                var ww = AchievementValueItems.Where(z => z.Achievement.Id == achId).ToArray();
                var dd = ww.Where(z => time > z.Timestamp && Math.Abs((z.Timestamp - time).TotalDays) < item.Days).ToArray();
                for (int i = 0; i < dd.Length; i++)
                {
                    ret *= item.Modifier;
                }
            }

            return ret;
        }
    }
}
