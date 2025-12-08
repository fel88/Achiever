using Achiever.Controllers;
using Microsoft.EntityFrameworkCore;
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
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=achiever.db");
    }
}
