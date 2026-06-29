using System.Security.Claims;
using FamiHub.API.DTOs;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/family-events")]
    public class FamilyEventsController : ControllerBase
    {
        private readonly FamilyEventService _familyEventService;

        public FamilyEventsController(FamilyEventService familyEventService)
        {
            _familyEventService = familyEventService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "";
        private async Task<int?> GetFamilyIdAsync()
        {
            var familyIdStr = User.FindFirst("FamilyId")?.Value;
            if (!string.IsNullOrEmpty(familyIdStr) && int.TryParse(familyIdStr, out var id)) return id;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
            {
                var db = HttpContext.RequestServices.GetRequiredService<FamiHub.API.Data.AppDbContext>();
                var user = await db.Users.FindAsync(userId);
                return user?.FamilyId;
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var familyId = await GetFamilyIdAsync();
            if (familyId == null) return BadRequest(new { message = "Bạn chưa tham gia gia đình nào." });

            var events = await _familyEventService.GetEventsAsync(familyId.Value);
            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateFamilyEventDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Tiêu đề không được để trống." });

            var familyEvent = await _familyEventService.CreateEventAsync(GetUserId(), dto);
            if (familyEvent == null) return BadRequest(new { message = "Lỗi khi tạo sự kiện." });

            return Ok(familyEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (GetRole() != "Parent")
                return Forbid();

            var success = await _familyEventService.DeleteEventAsync(id, GetUserId());
            if (!success) return NotFound(new { message = "Không tìm thấy sự kiện." });

            return NoContent();
        }
    }
}
