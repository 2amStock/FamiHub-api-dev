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
        }
    }
}

