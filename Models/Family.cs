using System.ComponentModel.DataAnnotations;

namespace FamiHub.API.Models
{
    public class Family
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string InviteCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Members { get; set; } = new List<User>();
        public ICollection<FamilyTask> Tasks { get; set; } = new List<FamilyTask>();
    }
}
