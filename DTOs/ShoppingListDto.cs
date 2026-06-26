using FamiHub.API.Models;

namespace FamiHub.API.DTOs
{
    public class ShoppingItemDto
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string? Unit { get; set; }
        public bool IsBought { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ShoppingListDto
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ShoppingItemDto> Items { get; set; } = new List<ShoppingItemDto>();
    }

    public class CreateShoppingItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; } = 1;
        public string? Unit { get; set; }
    }

    public class UpdateShoppingItemDto
    {
        public string? Name { get; set; }
        public double? Quantity { get; set; }
        public string? Unit { get; set; }
        public bool? IsBought { get; set; }
    }
}
