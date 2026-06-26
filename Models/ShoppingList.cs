using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class ShoppingList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public Family? Family { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "Danh sách tuần này";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "active"; // active, archived

        public DateTime CreatedAt { get; set; } = FamiHub.API.Utils.AppTime.Now;

        public DateTime UpdatedAt { get; set; } = FamiHub.API.Utils.AppTime.Now;

        // Navigation property
        public ICollection<ShoppingItem> Items { get; set; } = new List<ShoppingItem>();
    }
}
