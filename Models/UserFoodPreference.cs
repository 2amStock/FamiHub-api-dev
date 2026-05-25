using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class UserFoodPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string? FavoriteDishes { get; set; } // JSON array: ["Phở", "Bún bò"]

        public string? DislikedIngredients { get; set; } // JSON array: ["Mùi tàu", "Hành"]

        public string? DietaryRestrictions { get; set; } // JSON array: ["Chay", "Không gluten"]

        public string? CuisinePreferences { get; set; } // JSON array: ["Việt Nam", "Hàn Quốc"]

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
