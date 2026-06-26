using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamiHub.API.Models
{
    public class ShoppingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ListId { get; set; }

        [ForeignKey("ListId")]
        public ShoppingList? ShoppingList { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public double Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string? Unit { get; set; }

        public bool IsBought { get; set; } = false;

        public int? BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = FamiHub.API.Utils.AppTime.Now;
    }
}
