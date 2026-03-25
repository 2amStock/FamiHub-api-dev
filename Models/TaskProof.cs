using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class TaskProof
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public FamilyTask? Task { get; set; }

        [Required]
        public int ChildUserId { get; set; }

        [ForeignKey("ChildUserId")]
        public User? Child { get; set; }

        [Required]
        public string PhotoUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
