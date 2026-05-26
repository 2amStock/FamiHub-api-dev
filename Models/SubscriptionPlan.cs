using System.ComponentModel.DataAnnotations;

namespace FamiHub.API.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., FREE, STARTER, FAMILY
        
        public decimal Price { get; set; }
        
        [MaxLength(20)]
        public string DurationType { get; set; } = "MONTHLY"; 
        
        public int MaxMembers { get; set; }
        public int MaxTasksPerDay { get; set; }
        public bool HasAI { get; set; }
        public bool HasCalendar { get; set; }
        public bool HasShoppingList { get; set; }
        public bool HasStudyTracking { get; set; }
        public bool HasAchievement { get; set; }
    }
}
