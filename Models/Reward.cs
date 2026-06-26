using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        public int? FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public Family? Family { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int RequiredPoints { get; set; }

        public DateTime CreatedAt { get; set; } = FamiHub.API.Utils.AppTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<RewardRedemption> Redemptions { get; set; } = new List<RewardRedemption>();

        public bool IsSuggested { get; set; } = false;
    }
}
