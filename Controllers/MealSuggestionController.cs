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
