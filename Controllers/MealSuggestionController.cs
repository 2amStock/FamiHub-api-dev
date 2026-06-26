using FamiHub.API.Models;
using FamiHub.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;
using FamiHub.API.DTOs;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/meals")]
    [Authorize]
    public class MealSuggestionController : ControllerBase
    {
        private readonly MealSuggestionService _mealService;

        public MealSuggestionController(MealSuggestionService mealService)
        {
            _mealService = mealService;
        }

        /// <summary>
        /// Gợi ý món ăn bằng AI — Chỉ Parent mới có quyền (tiết kiệm token)
        /// </summary>
        [HttpPost("suggest")]
        public async Task<IActionResult> SuggestMeals([FromBody] MealSuggestionRequestDto request)
        {
            // Phân quyền: chỉ Parent mới được gọi AI
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Parent")
            {
                return StatusCode(403, new { message = "Chỉ phụ huynh mới có quyền sử dụng tính năng gợi ý món ăn AI." });
            }

            var userId = GetUserId();
            var familyId = GetFamilyId();
            if (familyId == null)
                return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            // Kiểm tra gói cước có hỗ trợ AI không
            var db = HttpContext.RequestServices.GetRequiredService<FamiHub.API.Data.AppDbContext>();
            var user = await db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var currentPlan = await db.SubscriptionPlans.FindAsync(user.CurrentPlanId);
            if (currentPlan == null || !currentPlan.HasAI)
            {
                return StatusCode(403, new { message = "Gói cước hiện tại của bạn không hỗ trợ tính năng AI. Vui lòng nâng cấp lên gói Family hoặc Yearly để sử dụng tính năng này." });
            }

            var results = await _mealService.SuggestMealsAsync(request, userId, familyId.Value);

            if (results.Count == 0)
                return StatusCode(500, new { message = "Không thể gợi ý món ăn lúc này. Vui lòng thử lại sau." });

            return Ok(new { message = "Gợi ý món ăn thành công!", dishes = results });
        }

        /// <summary>
        /// Lấy lịch sử gợi ý của gia đình — Tất cả thành viên
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var familyId = GetFamilyId();
            if (familyId == null)
                return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var results = await _mealService.GetHistoryAsync(familyId.Value, page, pageSize);
            return Ok(results);
        }

        /// <summary>
        /// Lấy danh sách món yêu thích — Tất cả thành viên
        /// </summary>
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var familyId = GetFamilyId();
            if (familyId == null)
                return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var results = await _mealService.GetFavoritesAsync(familyId.Value);
            return Ok(results);
        }

        /// <summary>
        /// Toggle yêu thích — Tất cả thành viên
        /// </summary>
        [HttpPut("{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var familyId = GetFamilyId();
            if (familyId == null)
                return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var result = await _mealService.ToggleFavoriteAsync(id, familyId.Value);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy gợi ý món ăn." });

            return Ok(new { message = result.IsFavorite ? "Đã thêm vào yêu thích!" : "Đã bỏ yêu thích.", dish = result });
        }

        /// <summary>
        /// Xóa gợi ý — Chỉ Parent mới có quyền
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSuggestion(int id)
        {
            // Phân quyền: chỉ Parent
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Parent")
            {
                return StatusCode(403, new { message = "Chỉ phụ huynh mới có quyền xóa gợi ý món ăn." });
            }

            var familyId = GetFamilyId();
            if (familyId == null)
                return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var success = await _mealService.DeleteSuggestionAsync(id, familyId.Value);
            if (!success)
                return NotFound(new { message = "Không tìm thấy gợi ý món ăn." });

            return Ok(new { message = "Đã xóa gợi ý món ăn." });
        }

        /// <summary>
        /// Lấy sở thích ẩm thực của bản thân — Tất cả thành viên
        /// </summary>
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var userId = GetUserId();
            var result = await _mealService.GetPreferenceAsync(userId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật sở thích ẩm thực của bản thân — Tất cả thành viên
        /// </summary>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdateFoodPreferenceDto dto)
        {
            var userId = GetUserId();
            var result = await _mealService.UpdatePreferenceAsync(userId, dto);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });

            return Ok(new { message = "Đã cập nhật sở thích ẩm thực!", preferences = result });
        }

        /// <summary>
        /// Thêm nguyên liệu của món ăn vào Shopping List
        /// </summary>
        [HttpPost("{id}/add-to-shopping-list")]
        public async Task<IActionResult> AddToShoppingList(int id)
        {
            var userId = GetUserId();
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var db = HttpContext.RequestServices.GetRequiredService<FamiHub.API.Data.AppDbContext>();
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ShoppingHub>>();

            // Check subscription
            var user = await db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();
            var currentPlan = await db.SubscriptionPlans.FindAsync(user.CurrentPlanId);
            if (currentPlan == null || !currentPlan.HasShoppingList)
            {
                return StatusCode(403, new { message = "Gói cước hiện tại của bạn không hỗ trợ Shopping List." });
            }

            var meal = await db.MealSuggestions.FirstOrDefaultAsync(m => m.Id == id && m.FamilyId == familyId.Value);
            if (meal == null) return NotFound(new { message = "Không tìm thấy món ăn." });

            // Parse ingredients (assuming JSON array of objects or strings)
            // Example: [{"Name": "Thịt bò", "Amount": "500g"}] or just simple array
            List<string> ingredientsList = new List<string>();
            try
            {
                // Attempt to parse. For simplicity, if it's a JSON string array.
                // Depending on the AI format, we might need a robust parser.
                var parsed = JsonSerializer.Deserialize<List<JsonElement>>(meal.Ingredients);
                if (parsed != null)
                {
                    foreach (var item in parsed)
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            ingredientsList.Add(item.GetString()!);
                        }
                        else if (item.ValueKind == JsonValueKind.Object)
                        {
                            // If it's an object like { "name": "Thịt bò", "quantity": "500g" }
                            var name = item.TryGetProperty("name", out var n) ? n.GetString() : "";
                            var qty = item.TryGetProperty("quantity", out var q) ? q.GetString() : "";
                            ingredientsList.Add($"{name} {qty}".Trim());
                        }
                    }
                }
            }
            catch
            {
                ingredientsList.Add(meal.Ingredients); // Fallback
            }

            // Get or create active shopping list
            var activeList = await db.ShoppingLists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.FamilyId == familyId.Value && l.Status == "active");

            if (activeList == null)
            {
                activeList = new ShoppingList
                {
                    FamilyId = familyId.Value,
                    Name = "Danh sách tuần này",
                    Status = "active"
                };
                db.ShoppingLists.Add(activeList);
                await db.SaveChangesAsync();
            }

            var addedItems = new List<ShoppingItemDto>();

            foreach (var ingName in ingredientsList)
            {
                if (string.IsNullOrWhiteSpace(ingName)) continue;

                var newItem = new ShoppingItem
                {
                    ListId = activeList.Id,
                    Name = ingName,
                    Quantity = 1,
                    Unit = "",
                    CreatedByUserId = userId
                };
                db.ShoppingItems.Add(newItem);
                await db.SaveChangesAsync();

                addedItems.Add(new ShoppingItemDto
                {
                    Id = newItem.Id,
                    ListId = newItem.ListId,
                    Name = newItem.Name,
                    Quantity = newItem.Quantity,
                    Unit = newItem.Unit,
                    IsBought = newItem.IsBought,
                    BuyerId = newItem.BuyerId,
                    CreatedByUserId = newItem.CreatedByUserId,
                    CreatedAt = newItem.CreatedAt
                });

                // Notify clients
                await hubContext.Groups.ClientGroup($"Family_{familyId.Value}").SendAsync("ShoppingItemAdded", addedItems.Last());
            }

            activeList.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
            await db.SaveChangesAsync();

            return Ok(new { message = $"Đã thêm {addedItems.Count} nguyên liệu vào Shopping List.", items = addedItems });
        }

        // ========== Helpers ==========

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private int? GetFamilyId()
        {
            var familyIdClaim = User.FindFirstValue("FamilyId");
            if (string.IsNullOrEmpty(familyIdClaim)) return null;
            return int.TryParse(familyIdClaim, out var id) ? id : null;
        }
    }
}
