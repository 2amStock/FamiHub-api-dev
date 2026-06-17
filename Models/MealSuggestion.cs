using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class MealSuggestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public Family? Family { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }

        [ForeignKey("RequestedByUserId")]
        public User? RequestedBy { get; set; }

        [Required]
        [MaxLength(50)]
        public string MealType { get; set; } = "Lunch"; // Breakfast, Lunch, Dinner, Snack

        [Required]
        [MaxLength(200)]
        public string DishName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Ingredients { get; set; } = "[]"; // JSON array of ingredients

        [Required]
        public string Instructions { get; set; } = "[]"; // JSON array of steps

        public int ServingSize { get; set; } = 4;

        public int EstimatedTime { get; set; } = 30; // minutes

        [MaxLength(50)]
        public string? DifficultyLevel { get; set; } // Dễ, Trung bình, Khó

        [MaxLength(100)]
        public string? CuisineType { get; set; } // Việt Nam, Âu, Á...

        public string? NutritionInfo { get; set; } // JSON nutrition data

        public bool IsFavorite { get; set; } = false;

        public DateTime CreatedAt { get; set; } = FamiHub.API.Utils.AppTime.Now;
    }
}
