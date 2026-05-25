using System.Text.Json;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Services
{
    public class MealSuggestionService
    {
        private readonly AppDbContext _db;
        private readonly GeminiApiService _geminiApi;
        private readonly ILogger<MealSuggestionService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public MealSuggestionService(AppDbContext db, GeminiApiService geminiApi, ILogger<MealSuggestionService> logger)
        {
            _db = db;
            _geminiApi = geminiApi;
            _logger = logger;
        }

        /// <summary>
        /// Gợi ý món ăn bằng AI dựa trên sở thích gia đình và yêu cầu
        /// </summary>
        public async Task<List<MealSuggestionDto>> SuggestMealsAsync(
            MealSuggestionRequestDto request, int userId, int familyId)
        {
            // 1. Lấy sở thích ẩm thực của các thành viên trong gia đình
            var familyPreferences = await _db.UserFoodPreferences
                .Include(p => p.User)
                .Where(p => p.User != null && p.User.FamilyId == familyId)
                .ToListAsync();

            // 2. Lấy lịch sử gợi ý gần đây (7 ngày) để tránh lặp
            var recentSuggestions = await _db.MealSuggestions
                .Where(m => m.FamilyId == familyId && m.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .Select(m => m.DishName)
                .ToListAsync();

            // 3. Xây dựng prompt
            var prompt = BuildPrompt(request, familyPreferences, recentSuggestions);

            // 4. Gọi Gemini API
            var aiResponse = await _geminiApi.GenerateContentAsync(prompt);
            if (string.IsNullOrEmpty(aiResponse))
            {
                _logger.LogError("Gemini API không trả về kết quả");
                return new List<MealSuggestionDto>();
            }

            // 5. Parse kết quả JSON
            var dishes = ParseAiResponse(aiResponse);
            if (dishes == null || dishes.Count == 0)
            {
                _logger.LogError("Không thể parse kết quả từ AI: {Response}", aiResponse);
                return new List<MealSuggestionDto>();
            }

            // 6. Lưu vào database
            var results = new List<MealSuggestionDto>();
            foreach (var dish in dishes)
            {
                var suggestion = new MealSuggestion
                {
                    FamilyId = familyId,
                    RequestedByUserId = userId,
                    MealType = request.MealType,
                    DishName = dish.DishName ?? "Không xác định",
                    Description = dish.Description,
                    Ingredients = JsonSerializer.Serialize(dish.Ingredients ?? new List<IngredientDto>(), JsonOptions),
                    Instructions = JsonSerializer.Serialize(dish.Instructions ?? new List<string>(), JsonOptions),
                    ServingSize = request.ServingSize,
                    EstimatedTime = dish.EstimatedTime,
                    DifficultyLevel = dish.DifficultyLevel,
                    CuisineType = dish.CuisineType,
                    NutritionInfo = dish.NutritionInfo != null
                        ? JsonSerializer.Serialize(dish.NutritionInfo, JsonOptions)
                        : null,
                    CreatedAt = DateTime.UtcNow
                };

                _db.MealSuggestions.Add(suggestion);
                await _db.SaveChangesAsync();

                dish.Id = suggestion.Id;
                dish.MealType = request.MealType;
                dish.ServingSize = request.ServingSize;
                dish.IsFavorite = false;
                dish.CreatedAt = suggestion.CreatedAt;
                results.Add(dish);
            }

            return results;
        }

        /// <summary>
        /// Lấy lịch sử gợi ý của gia đình
        /// </summary>
        public async Task<List<MealSuggestionDto>> GetHistoryAsync(int familyId, int page = 1, int pageSize = 20)
        {
            var suggestions = await _db.MealSuggestions
                .Include(m => m.RequestedBy)
                .Where(m => m.FamilyId == familyId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return suggestions.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Lấy danh sách yêu thích
        /// </summary>
        public async Task<List<MealSuggestionDto>> GetFavoritesAsync(int familyId)
        {
            var favorites = await _db.MealSuggestions
                .Include(m => m.RequestedBy)
                .Where(m => m.FamilyId == familyId && m.IsFavorite)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return favorites.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Toggle yêu thích
        /// </summary>
        public async Task<MealSuggestionDto?> ToggleFavoriteAsync(int id, int familyId)
        {
            var suggestion = await _db.MealSuggestions
                .Include(m => m.RequestedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.FamilyId == familyId);

            if (suggestion == null) return null;

            suggestion.IsFavorite = !suggestion.IsFavorite;
            await _db.SaveChangesAsync();

            return MapToDto(suggestion);
        }

        /// <summary>
        /// Xóa gợi ý
        /// </summary>
        public async Task<bool> DeleteSuggestionAsync(int id, int familyId)
        {
            var suggestion = await _db.MealSuggestions
                .FirstOrDefaultAsync(m => m.Id == id && m.FamilyId == familyId);

            if (suggestion == null) return false;

            _db.MealSuggestions.Remove(suggestion);
            await _db.SaveChangesAsync();
            return true;
        }

        // ========== Food Preferences ==========

        /// <summary>
        /// Lấy sở thích ẩm thực của user
        /// </summary>
        public async Task<FoodPreferenceDto?> GetPreferenceAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var pref = await _db.UserFoodPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return new FoodPreferenceDto
            {
                UserId = userId,
                UserName = user.Name,
                FavoriteDishes = DeserializeList(pref?.FavoriteDishes),
                DislikedIngredients = DeserializeList(pref?.DislikedIngredients),
                DietaryRestrictions = DeserializeList(pref?.DietaryRestrictions),
                CuisinePreferences = DeserializeList(pref?.CuisinePreferences)
            };
        }

        /// <summary>
        /// Cập nhật sở thích ẩm thực
        /// </summary>
        public async Task<FoodPreferenceDto?> UpdatePreferenceAsync(int userId, UpdateFoodPreferenceDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var pref = await _db.UserFoodPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pref == null)
            {
                pref = new UserFoodPreference { UserId = userId };
                _db.UserFoodPreferences.Add(pref);
            }

            if (dto.FavoriteDishes != null)
                pref.FavoriteDishes = JsonSerializer.Serialize(dto.FavoriteDishes, JsonOptions);
            if (dto.DislikedIngredients != null)
                pref.DislikedIngredients = JsonSerializer.Serialize(dto.DislikedIngredients, JsonOptions);
            if (dto.DietaryRestrictions != null)
                pref.DietaryRestrictions = JsonSerializer.Serialize(dto.DietaryRestrictions, JsonOptions);
            if (dto.CuisinePreferences != null)
                pref.CuisinePreferences = JsonSerializer.Serialize(dto.CuisinePreferences, JsonOptions);

            pref.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new FoodPreferenceDto
            {
                UserId = userId,
                UserName = user.Name,
                FavoriteDishes = DeserializeList(pref.FavoriteDishes),
                DislikedIngredients = DeserializeList(pref.DislikedIngredients),
                DietaryRestrictions = DeserializeList(pref.DietaryRestrictions),
                CuisinePreferences = DeserializeList(pref.CuisinePreferences)
            };
        }

        // ========== Private Helpers ==========

        private string BuildPrompt(
            MealSuggestionRequestDto request,
            List<UserFoodPreference> familyPreferences,
            List<string> recentDishes)
        {
            var mealTypeVi = request.MealType switch
            {
                "Breakfast" => "bữa sáng",
                "Lunch" => "bữa trưa",
                "Dinner" => "bữa tối",
                "Snack" => "bữa phụ/ăn vặt",
                _ => "bữa ăn"
            };

            var prompt = $@"Bạn là một đầu bếp chuyên nghiệp người Việt Nam. Hãy gợi ý {request.NumberOfDishes} món ăn cho {mealTypeVi}, phục vụ {request.ServingSize} người.

YÊU CẦU:
- Gợi ý món ăn phù hợp với bữa ăn được yêu cầu
- Mỗi món phải có đầy đủ nguyên liệu, cách chế biến chi tiết từng bước
- Trả về bằng tiếng Việt";

            // Thêm thông tin nguyên liệu có sẵn
            if (!string.IsNullOrWhiteSpace(request.AvailableIngredients))
            {
                prompt += $"\n- Ưu tiên sử dụng các nguyên liệu có sẵn: {request.AvailableIngredients}";
            }

            // Thêm sở thích ẩm thực
            if (!string.IsNullOrWhiteSpace(request.CuisinePreference))
            {
                prompt += $"\n- Ẩm thực ưa thích: {request.CuisinePreference}";
            }

            // Thêm ghi chú
            if (!string.IsNullOrWhiteSpace(request.AdditionalNotes))
            {
                prompt += $"\n- Ghi chú thêm: {request.AdditionalNotes}";
            }

            // Sở thích từng thành viên gia đình
            if (familyPreferences.Any())
            {
                prompt += "\n\nSỞ THÍCH GIA ĐÌNH:";
                foreach (var pref in familyPreferences)
                {
                    var name = pref.User?.Name ?? "Thành viên";
                    var likes = DeserializeList(pref.FavoriteDishes);
                    var dislikes = DeserializeList(pref.DislikedIngredients);
                    var restrictions = DeserializeList(pref.DietaryRestrictions);

                    if (likes.Any())
                        prompt += $"\n- {name} thích: {string.Join(", ", likes)}";
                    if (dislikes.Any())
                        prompt += $"\n- {name} KHÔNG thích: {string.Join(", ", dislikes)}";
                    if (restrictions.Any())
                        prompt += $"\n- {name} hạn chế: {string.Join(", ", restrictions)}";
                }
            }

            // Tránh lặp lại món gần đây
            if (recentDishes.Any())
            {
                prompt += $"\n\nCÁC MÓN ĐÃ GỢI Ý GẦN ĐÂY (TRÁNH LẶP): {string.Join(", ", recentDishes)}";
            }

            // Yêu cầu format JSON
            prompt += @"

TRẢ VỀ ĐÚNG JSON FORMAT SAU (không thêm markdown, chỉ JSON thuần):
{
  ""dishes"": [
    {
      ""dishName"": ""Tên món ăn"",
      ""description"": ""Mô tả ngắn về món ăn"",
      ""ingredients"": [
        { ""name"": ""Tên nguyên liệu"", ""amount"": ""Số lượng"", ""unit"": ""Đơn vị"", ""note"": ""Ghi chú nếu có"" }
      ],
      ""instructions"": [
        ""Bước 1: ....."",
        ""Bước 2: .....""
      ],
      ""estimatedTime"": 30,
      ""difficultyLevel"": ""Dễ"",
      ""cuisineType"": ""Việt Nam"",
      ""nutritionInfo"": {
        ""calories"": ""~500 kcal"",
        ""protein"": ""~25g"",
        ""carbs"": ""~60g"",
        ""fat"": ""~15g""
      }
    }
  ]
}";

            return prompt;
        }

        private List<MealSuggestionDto>? ParseAiResponse(string aiResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiResponse);
                var root = doc.RootElement;

                if (!root.TryGetProperty("dishes", out var dishesArray))
                {
                    _logger.LogWarning("AI response thiếu property 'dishes'");
                    return null;
                }

                var results = new List<MealSuggestionDto>();

                foreach (var dish in dishesArray.EnumerateArray())
                {
                    var dto = new MealSuggestionDto
                    {
                        DishName = dish.GetProperty("dishName").GetString() ?? "Không xác định",
                        Description = dish.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        EstimatedTime = dish.TryGetProperty("estimatedTime", out var time) ? time.GetInt32() : 30,
                        DifficultyLevel = dish.TryGetProperty("difficultyLevel", out var diff) ? diff.GetString() : null,
                        CuisineType = dish.TryGetProperty("cuisineType", out var cuisine) ? cuisine.GetString() : null,
                    };

                    // Parse ingredients
                    if (dish.TryGetProperty("ingredients", out var ingredients))
                    {
                        dto.Ingredients = JsonSerializer.Deserialize<List<IngredientDto>>(
                            ingredients.GetRawText(), JsonOptions) ?? new List<IngredientDto>();
                    }

                    // Parse instructions
                    if (dish.TryGetProperty("instructions", out var instructions))
                    {
                        dto.Instructions = JsonSerializer.Deserialize<List<string>>(
                            instructions.GetRawText(), JsonOptions) ?? new List<string>();
                    }

                    // Parse nutrition info
                    if (dish.TryGetProperty("nutritionInfo", out var nutrition))
                    {
                        dto.NutritionInfo = JsonSerializer.Deserialize<NutritionInfoDto>(
                            nutrition.GetRawText(), JsonOptions);
                    }

                    results.Add(dto);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI response: {Response}", aiResponse);
                return null;
            }
        }

        private MealSuggestionDto MapToDto(MealSuggestion m)
        {
            return new MealSuggestionDto
            {
                Id = m.Id,
                MealType = m.MealType,
                DishName = m.DishName,
                Description = m.Description,
                Ingredients = DeserializeJson<List<IngredientDto>>(m.Ingredients) ?? new List<IngredientDto>(),
                Instructions = DeserializeJson<List<string>>(m.Instructions) ?? new List<string>(),
                ServingSize = m.ServingSize,
                EstimatedTime = m.EstimatedTime,
                DifficultyLevel = m.DifficultyLevel,
                CuisineType = m.CuisineType,
                NutritionInfo = DeserializeJson<NutritionInfoDto>(m.NutritionInfo),
                IsFavorite = m.IsFavorite,
                CreatedAt = m.CreatedAt,
                RequestedByName = m.RequestedBy?.Name
            };
        }

        private static List<string> DeserializeList(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static T? DeserializeJson<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }
    }
}
