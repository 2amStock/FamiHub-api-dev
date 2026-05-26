using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class UserSubscription
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int SubscriptionPlanId { get; set; }
        
        [ForeignKey("SubscriptionPlanId")]
        public SubscriptionPlan? Plan { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, EXPIRED, CANCELLED

        [MaxLength(100)]
        public string? PayOSOrderCode { get; set; }
        
        [MaxLength(100)]
        public string? TransactionId { get; set; }
    }
}
