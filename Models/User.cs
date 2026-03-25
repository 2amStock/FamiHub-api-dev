using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public enum UserRole
    {
        Parent,
        Child
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        public int? FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public Family? Family { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? Avatar { get; set; }

        public int Points { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FamilyTask> CreatedTasks { get; set; } = new List<FamilyTask>();
        public ICollection<FamilyTask> AssignedTasks { get; set; } = new List<FamilyTask>();
        public ICollection<TaskProof> Proofs { get; set; } = new List<TaskProof>();
    }
}
