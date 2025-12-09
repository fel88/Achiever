using Achiever.Controllers;
using Achiever.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Achiever.Model
{
    public class AchieverContext : DbContext
    {
        public AchieverContext()
        {
            Database.EnsureCreated();
            if (!Users.Any() || !Users.Any(z => z.Login == "local_admin"))
            {
                Users.Add(new User() { Enabled = true, IsAdmin = true, Login = "local_admin", Name = "local_admin", Password = LoginController.ComputeSha256Hash("12345") });
                SaveChanges();
            }
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
            var connectionString = ConfigLoader.ReadSetting("dbConnectionString");
            if (dbProvider == "postgres")
            {                
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString ?? "Data Source=achiever.db");
            }

        }
    }

}
