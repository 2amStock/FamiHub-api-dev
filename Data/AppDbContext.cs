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
        }
    }
}
