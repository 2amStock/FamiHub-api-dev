using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Submitted,
        Approved,
        Rejected
    }

    public class FamilyTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public Family? Family { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public int? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public User? AssignedTo { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public int Points { get; set; } = 10;

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public string? RejectionNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public TaskProof? Proof { get; set; }
    }
}
