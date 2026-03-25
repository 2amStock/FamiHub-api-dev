using System.Security.Claims;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public FamiliesController(AppDbContext db)
        {
            _db = db;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyDto dto)
        {
            var userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();
            if (user.Role != UserRole.Parent)
                return Forbid();

            var inviteCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
            var family = new Family { Name = dto.Name, InviteCode = inviteCode };
            _db.Families.Add(family);
            await _db.SaveChangesAsync();

            user.FamilyId = family.Id;
            await _db.SaveChangesAsync();

            return Ok(new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                InviteCode = family.InviteCode,
                Members = new List<UserDto> { AuthService.MapToDto(user) }
            });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinFamily([FromBody] JoinFamilyDto dto)
        {
            var userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();
            if (user.FamilyId != null)
                return BadRequest(new { message = "Bạn đã thuộc một gia đình." });

            var family = await _db.Families.FirstOrDefaultAsync(f => f.InviteCode == dto.InviteCode.ToUpper());
            if (family == null)
                return NotFound(new { message = "Mã mời không hợp lệ." });

            user.FamilyId = family.Id;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Tham gia gia đình thành công!", familyId = family.Id, familyName = family.Name });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyFamily()
        {
            var userId = GetUserId();
            var user = await _db.Users.Include(u => u.Family).FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Family == null)
                return NotFound(new { message = "Bạn chưa có gia đình." });

            var members = await _db.Users
                .Where(u => u.FamilyId == user.FamilyId)
                .ToListAsync();

            return Ok(new FamilyDto
            {
                Id = user.Family.Id,
                Name = user.Family.Name,
                InviteCode = user.Family.InviteCode,
                Members = members.Select(AuthService.MapToDto).ToList()
            });
        }
    }
}
