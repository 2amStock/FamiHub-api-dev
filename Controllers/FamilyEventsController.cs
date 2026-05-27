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
        private int? GetFamilyId()
        {
            var familyIdStr = User.FindFirst("FamilyId")?.Value;
            if (string.IsNullOrEmpty(familyIdStr)) return null;
            return int.Parse(familyIdStr);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var familyId = GetFamilyId();
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
