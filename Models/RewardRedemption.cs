using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public enum RedemptionStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class RewardRedemption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RewardId { get; set; }

        [ForeignKey("RewardId")]
        public Reward? Reward { get; set; }

        [Required]
        public int ChildUserId { get; set; }

        [ForeignKey("ChildUserId")]
        public User? Child { get; set; }

        [Required]
        public RedemptionStatus Status { get; set; } = RedemptionStatus.Pending;

        [MaxLength(500)]
        public string? ParentNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
