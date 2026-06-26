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

            // Parse ingredients from AI JSON: [{name, amount, unit}, ...]
            var parsedIngredients = new List<(string Name, double Quantity, string Unit)>();
            try
            {
                var parsed = JsonSerializer.Deserialize<List<JsonElement>>(meal.Ingredients);
                if (parsed != null)
                {
                    foreach (var item in parsed)
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            parsedIngredients.Add((item.GetString()!, 1, ""));
                        }
                        else if (item.ValueKind == JsonValueKind.Object)
                        {
                            var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                            var amountStr = item.TryGetProperty("amount", out var a) ? a.GetString() ?? "1" : "1";
                            var unit = item.TryGetProperty("unit", out var u) ? u.GetString() ?? "" : "";

                            // Parse numeric value from amount string (e.g., "500" or "2.5")
                            double quantity = 1;
                            // Try to extract number from the amount string
                            var numStr = System.Text.RegularExpressions.Regex.Match(amountStr, @"[\d.,]+").Value;
                            if (!string.IsNullOrEmpty(numStr))
                            {
                                numStr = numStr.Replace(",", ".");
                                double.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out quantity);
                                if (quantity <= 0) quantity = 1;
                            }

                            // If unit is empty but amount has text suffix, extract it
                            if (string.IsNullOrWhiteSpace(unit))
                            {
                                var unitFromAmount = System.Text.RegularExpressions.Regex.Replace(amountStr, @"[\d.,\s]+", "").Trim();
                                if (!string.IsNullOrEmpty(unitFromAmount))
                                    unit = unitFromAmount;
                            }

                            if (!string.IsNullOrWhiteSpace(name))
                                parsedIngredients.Add((name.Trim(), quantity, unit.Trim()));
                        }
                    }
                }
            }
            catch
            {
                parsedIngredients.Add((meal.Ingredients, 1, ""));
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

            var addedOrUpdatedItems = new List<ShoppingItemDto>();

            foreach (var ing in parsedIngredients)
            {
                if (string.IsNullOrWhiteSpace(ing.Name)) continue;

                // Check if an item with the same name and unit already exists in the active list
                var existingItem = activeList.Items.FirstOrDefault(i =>
                    i.Name.Equals(ing.Name, StringComparison.OrdinalIgnoreCase) &&
                    (i.Unit ?? "").Equals(ing.Unit, StringComparison.OrdinalIgnoreCase) &&
                    !i.IsBought);

                if (existingItem != null)
                {
                    // Accumulate quantity
                    existingItem.Quantity += ing.Quantity;
                    await db.SaveChangesAsync();

                    var dto = new ShoppingItemDto
                    {
                        Id = existingItem.Id,
                        ListId = existingItem.ListId,
                        Name = existingItem.Name,
                        Quantity = existingItem.Quantity,
                        Unit = existingItem.Unit,
                        IsBought = existingItem.IsBought,
                        BuyerId = existingItem.BuyerId,
                        CreatedByUserId = existingItem.CreatedByUserId,
                        CreatedAt = existingItem.CreatedAt
                    };
                    addedOrUpdatedItems.Add(dto);

                    // Notify as updated (not added)
                    await hubContext.Clients.Group($"Family_{familyId.Value}").SendAsync("ShoppingItemUpdated", dto);
                }
                else
                {
                    var newItem = new ShoppingItem
                    {
                        ListId = activeList.Id,
                        Name = ing.Name,
                        Quantity = ing.Quantity,
                        Unit = ing.Unit,
                        CreatedByUserId = userId
                    };
                    db.ShoppingItems.Add(newItem);
                    await db.SaveChangesAsync();
                    activeList.Items.Add(newItem);

                    var dto = new ShoppingItemDto
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
                    };
                    addedOrUpdatedItems.Add(dto);

                    // Notify as added
                    await hubContext.Clients.Group($"Family_{familyId.Value}").SendAsync("ShoppingItemAdded", dto);
                }
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
