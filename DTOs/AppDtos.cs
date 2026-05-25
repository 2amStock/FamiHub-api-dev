namespace FamiHub.API.DTOs
{
    // Auth DTOs
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Child"; // Parent or Child
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? FamilyId { get; set; }
        public string? FamilyName { get; set; }
        public int Points { get; set; }
        public string? Avatar { get; set; }
    }

    // Family DTOs
    public class CreateFamilyDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class JoinFamilyDto
    {
        public string InviteCode { get; set; } = string.Empty;
    }

    public class FamilyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public List<UserDto> Members { get; set; } = new();
    }

    // Task DTOs
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int Points { get; set; } = 10;
    }

    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Points { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Points { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? AssignedTo { get; set; }
        public UserDto? CreatedBy { get; set; }
        public TaskProofDto? Proof { get; set; }
        public string? RejectionNote { get; set; }
    }

    public class ApproveTaskDto
    {
        public bool Approved { get; set; }
        public string? RejectionNote { get; set; }
    }

    public class SubmitTaskDto
    {
        public string? Note { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    // Proof DTOs
    public class TaskProofDto
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime SubmittedAt { get; set; }
        public UserDto? Child { get; set; }
    }

    // ========== Meal Suggestion DTOs ==========

    // Request gợi ý món ăn từ AI
    public class MealSuggestionRequestDto
    {
        public string MealType { get; set; } = "Lunch"; // Breakfast, Lunch, Dinner, Snack
        public int ServingSize { get; set; } = 4;
        public string? AvailableIngredients { get; set; } // Nguyên liệu có sẵn
        public string? CuisinePreference { get; set; } // Loại ẩm thực ưa thích
        public string? AdditionalNotes { get; set; } // Ghi chú (ăn kiêng, nhanh gọn...)
        public int NumberOfDishes { get; set; } = 3; // Số món muốn gợi ý
    }

    // Response gợi ý món ăn
    public class MealSuggestionDto
    {
        public int Id { get; set; }
        public string MealType { get; set; } = string.Empty;
        public string DishName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<IngredientDto> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public int ServingSize { get; set; }
        public int EstimatedTime { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? CuisineType { get; set; }
        public NutritionInfoDto? NutritionInfo { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RequestedByName { get; set; }
    }

    // Chi tiết nguyên liệu
    public class IngredientDto
    {
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Note { get; set; } // Ghi chú (có thể thay bằng...)
    }

    // Thông tin dinh dưỡng
    public class NutritionInfoDto
    {
        public string? Calories { get; set; }
        public string? Protein { get; set; }
        public string? Carbs { get; set; }
        public string? Fat { get; set; }
    }

    // Cập nhật sở thích ẩm thực
    public class UpdateFoodPreferenceDto
    {
        public List<string>? FavoriteDishes { get; set; }
        public List<string>? DislikedIngredients { get; set; }
        public List<string>? DietaryRestrictions { get; set; }
        public List<string>? CuisinePreferences { get; set; }
    }

    // Response sở thích ẩm thực
    public class FoodPreferenceDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> FavoriteDishes { get; set; } = new();
        public List<string> DislikedIngredients { get; set; } = new();
        public List<string> DietaryRestrictions { get; set; } = new();
        public List<string> CuisinePreferences { get; set; } = new();
    }
}

