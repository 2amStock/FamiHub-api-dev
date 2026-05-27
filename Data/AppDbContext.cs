using Microsoft.EntityFrameworkCore;
using FamiHub.API.Models;

namespace FamiHub.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Family> Families { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FamilyTask> Tasks { get; set; }
        public DbSet<TaskProof> TaskProofs { get; set; }
        public DbSet<MealSuggestion> MealSuggestions { get; set; }
        public DbSet<UserFoodPreference> UserFoodPreferences { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RewardRedemption> RewardRedemptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Family
            modelBuilder.Entity<Family>()
                .HasIndex(f => f.InviteCode).IsUnique();

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Family)
                .WithMany(f => f.Members)
                .HasForeignKey(u => u.FamilyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // FamilyTask
            modelBuilder.Entity<FamilyTask>()
                .HasOne(t => t.Family)
                .WithMany(f => f.Tasks)
                .HasForeignKey(t => t.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FamilyTask>()
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FamilyTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FamilyTask>()
                .Property(t => t.Status)
                .HasConversion<string>();

            // TaskProof
            modelBuilder.Entity<TaskProof>()
                .HasOne(p => p.Task)
                .WithOne(t => t.Proof)
                .HasForeignKey<TaskProof>(p => p.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProof>()
                .HasOne(p => p.Child)
                .WithMany(u => u.Proofs)
                .HasForeignKey(p => p.ChildUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MealSuggestion
            modelBuilder.Entity<MealSuggestion>()
                .HasOne(m => m.Family)
                .WithMany()
                .HasForeignKey(m => m.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MealSuggestion>()
                .HasOne(m => m.RequestedBy)
                .WithMany()
                .HasForeignKey(m => m.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserFoodPreference
            modelBuilder.Entity<UserFoodPreference>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFoodPreference>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Seed Subscription Plans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = 1, Name = "FREE", Price = 0m, DurationType = "MONTHLY", MaxMembers = 3, MaxTasksPerDay = 5, HasAI = false, HasCalendar = false, HasShoppingList = false, HasStudyTracking = false, HasAchievement = false },
                new SubscriptionPlan { Id = 2, Name = "STARTER", Price = 79000m, DurationType = "MONTHLY", MaxMembers = 5, MaxTasksPerDay = 999, HasAI = false, HasCalendar = false, HasShoppingList = false, HasStudyTracking = true, HasAchievement = false },
                new SubscriptionPlan { Id = 3, Name = "FAMILY", Price = 119000m, DurationType = "MONTHLY", MaxMembers = 999, MaxTasksPerDay = 999, HasAI = true, HasCalendar = true, HasShoppingList = true, HasStudyTracking = true, HasAchievement = true },
                new SubscriptionPlan { Id = 4, Name = "YEARLY", Price = 1199000m, DurationType = "YEARLY", MaxMembers = 999, MaxTasksPerDay = 999, HasAI = true, HasCalendar = true, HasShoppingList = true, HasStudyTracking = true, HasAchievement = true }
            );

            // Reward
            modelBuilder.Entity<Reward>()
                .HasOne(r => r.Family)
                .WithMany()
                .HasForeignKey(r => r.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Fixed Rewards
            modelBuilder.Entity<Reward>().HasData(
                new Reward { Id = -1, FamilyId = null, CreatedByUserId = null, Title = "Xem TV 30 phút", RequiredPoints = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Reward { Id = -2, FamilyId = null, CreatedByUserId = null, Title = "Chơi game 1 giờ", RequiredPoints = 100, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Reward { Id = -3, FamilyId = null, CreatedByUserId = null, Title = "Mua đồ ăn vặt", RequiredPoints = 150, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // RewardRedemption
            modelBuilder.Entity<RewardRedemption>()
                .HasOne(rr => rr.Reward)
                .WithMany(r => r.Redemptions)
                .HasForeignKey(rr => rr.RewardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RewardRedemption>()
                .HasOne(rr => rr.Child)
                .WithMany()
                .HasForeignKey(rr => rr.ChildUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RewardRedemption>()
                .Property(rr => rr.Status)
                .HasConversion<string>();
        }
    }
}

