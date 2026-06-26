using System.Security.Claims;
using FamiHub.API.DTOs;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RewardsController : ControllerBase
    {
        private readonly RewardService _rewardService;

        public RewardsController(RewardService rewardService)
        {
            _rewardService = rewardService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "";

        // === REWARDS MANAGEMENT ===

        [HttpGet]
        public async Task<IActionResult> GetRewards()
        {
            var rewards = await _rewardService.GetRewardsAsync(GetUserId());
            return Ok(rewards);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReward(int id)
        {
            var reward = await _rewardService.GetRewardByIdAsync(id, GetUserId());
            if (reward == null) return NotFound();
            return Ok(reward);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReward([FromBody] CreateRewardDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Tiêu đề phần thưởng không được trống." });

            if (dto.RequiredPoints <= 0)
                return BadRequest(new { message = "Số điểm yêu cầu phải lớn hơn 0." });

            try
            {
                var reward = await _rewardService.CreateRewardAsync(dto, GetUserId());
                if (reward == null)
                    return BadRequest(new { message = "Không thể tạo phần thưởng. Hãy đảm bảo bạn đã có gia đình." });

                return CreatedAtAction(nameof(GetReward), new { id = reward.Id }, reward);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> SuggestReward([FromBody] SuggestRewardDto dto)
        {
            if (GetRole() != "Child")
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Tiêu đề phần thưởng không được trống." });

            try
            {
                var reward = await _rewardService.SuggestRewardAsync(dto, GetUserId());
                if (reward == null)
                    return BadRequest(new { message = "Không thể tạo đề xuất. Hãy đảm bảo bạn đã có gia đình." });

                return CreatedAtAction(nameof(GetReward), new { id = reward.Id }, reward);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReward(int id, [FromBody] UpdateRewardDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            if (dto.RequiredPoints.HasValue && dto.RequiredPoints.Value <= 0)
                return BadRequest(new { message = "Số điểm yêu cầu phải lớn hơn 0." });

            try
            {
                var reward = await _rewardService.UpdateRewardAsync(id, dto, GetUserId());
                if (reward == null) return NotFound();
                return Ok(reward);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            if (GetRole() != "Parent")
                return Forbid();

            try
            {
                var result = await _rewardService.DeleteRewardAsync(id, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // === REDEMPTION MANAGEMENT ===

        [HttpGet("redemptions")]
        public async Task<IActionResult> GetRedemptions()
        {
            var redemptions = await _rewardService.GetRedemptionsAsync(GetUserId());
            return Ok(redemptions);
        }

        [HttpGet("redemptions/{id}")]
        public async Task<IActionResult> GetRedemption(int id)
        {
            var redemption = await _rewardService.GetRedemptionByIdAsync(id, GetUserId());
            if (redemption == null) return NotFound();
            return Ok(redemption);
        }

        [HttpPost("{id}/redeem")]
        public async Task<IActionResult> RedeemReward(int id)
        {
            if (GetRole() != "Child")
                return Forbid();

            try
            {
                var redemption = await _rewardService.RedeemRewardAsync(id, GetUserId());
                if (redemption == null)
                    return BadRequest(new { message = "Không thể đổi phần thưởng. Vui lòng kiểm tra lại thông tin." });
                return Ok(redemption);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("redemptions/{id}/approve")]
        public async Task<IActionResult> ApproveRedemption(int id, [FromBody] ApproveRedemptionDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            var redemption = await _rewardService.ApproveRedemptionAsync(id, dto, GetUserId());
            if (redemption == null)
                return BadRequest(new { message = "Không thể duyệt yêu cầu đổi quà này. Có thể yêu cầu đã được duyệt hoặc không tồn tại." });

            return Ok(redemption);
        }
    }
}
