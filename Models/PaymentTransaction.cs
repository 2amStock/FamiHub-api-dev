using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        public int SubscriptionPlanId { get; set; }
        
        [ForeignKey("SubscriptionPlanId")]
        public SubscriptionPlan? Plan { get; set; }

        [MaxLength(100)]
        public string OrderCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
