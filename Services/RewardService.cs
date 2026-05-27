using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Services
{
    public class RewardService
    {
        private readonly AppDbContext _db;

        public RewardService(AppDbContext db)
        {
            _db = db;
        }

        // ==================== REWARDS ====================

        public async Task<List<RewardDto>> GetRewardsAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return new List<RewardDto>();

            var rewards = await _db.Rewards
                .Include(r => r.CreatedBy)
                .Where(r => r.FamilyId == user.FamilyId || r.FamilyId == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return rewards.Select(MapToDto).ToList();
        }

        public async Task<RewardDto?> GetRewardByIdAsync(int rewardId, int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var reward = await _db.Rewards
                .Include(r => r.CreatedBy)
                .FirstOrDefaultAsync(r => r.Id == rewardId);

            if (reward == null || (reward.FamilyId != null && reward.FamilyId != user.FamilyId)) return null;

            return MapToDto(reward);
        }

        public async Task<RewardDto?> CreateRewardAsync(CreateRewardDto dto, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            if (parent == null || parent.FamilyId == null || parent.Role != UserRole.Parent) return null;

            if (parent.CurrentPlanId == 1)
                throw new InvalidOperationException("Gói Miễn Phí không hỗ trợ tạo phần thưởng. Vui lòng nâng cấp gói để sử dụng tính năng này.");

            var reward = new Reward
            {
                FamilyId = parent.FamilyId.Value,
                CreatedByUserId = parentUserId,
                Title = dto.Title,
                Description = dto.Description,
                RequiredPoints = dto.RequiredPoints,
                CreatedAt = DateTime.UtcNow
            };

            _db.Rewards.Add(reward);
            await _db.SaveChangesAsync();

            return await GetRewardByIdAsync(reward.Id, parentUserId);
        }

        public async Task<RewardDto?> UpdateRewardAsync(int rewardId, UpdateRewardDto dto, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            if (parent == null || parent.Role != UserRole.Parent) return null;

            if (parent.CurrentPlanId == 1)
                throw new InvalidOperationException("Gói Miễn Phí không hỗ trợ sửa phần thưởng. Vui lòng nâng cấp gói để sử dụng tính năng này.");

            var reward = await _db.Rewards.FindAsync(rewardId);
            if (reward == null || (reward.FamilyId != null && reward.FamilyId != parent.FamilyId)) return null;
            
            if (reward.FamilyId == null)
                throw new InvalidOperationException("Không thể sửa phần thưởng hệ thống.");

            if (dto.Title != null) reward.Title = dto.Title;
            if (dto.Description != null) reward.Description = dto.Description;
            if (dto.RequiredPoints.HasValue) reward.RequiredPoints = dto.RequiredPoints.Value;
            reward.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await GetRewardByIdAsync(rewardId, parentUserId);
        }

        public async Task<bool> DeleteRewardAsync(int rewardId, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            if (parent == null || parent.Role != UserRole.Parent) return false;

            if (parent.CurrentPlanId == 1)
                throw new InvalidOperationException("Gói Miễn Phí không hỗ trợ xóa phần thưởng. Vui lòng nâng cấp gói để sử dụng tính năng này.");

            var reward = await _db.Rewards.FindAsync(rewardId);
            if (reward == null || (reward.FamilyId != null && reward.FamilyId != parent.FamilyId)) return false;

            if (reward.FamilyId == null)
                throw new InvalidOperationException("Không thể xóa phần thưởng hệ thống.");

            _db.Rewards.Remove(reward);
            await _db.SaveChangesAsync();

            return true;
        }

        // ==================== REDEMPTIONS ====================

        public async Task<List<RewardRedemptionDto>> GetRedemptionsAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return new List<RewardRedemptionDto>();

            IQueryable<RewardRedemption> query = _db.RewardRedemptions
                .Include(rr => rr.Reward)
                .Include(rr => rr.Child)
                .Where(rr => rr.Reward != null && (rr.Reward.FamilyId == user.FamilyId || rr.Reward.FamilyId == null));

            if (user.Role == UserRole.Child)
            {
                query = query.Where(rr => rr.ChildUserId == userId);
            }

            var redemptions = await query.OrderByDescending(rr => rr.CreatedAt).ToListAsync();
            return redemptions.Select(MapToDto).ToList();
        }

        public async Task<RewardRedemptionDto?> GetRedemptionByIdAsync(int redemptionId, int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var redemption = await _db.RewardRedemptions
                .Include(rr => rr.Reward)
                .Include(rr => rr.Child)
                .FirstOrDefaultAsync(rr => rr.Id == redemptionId);

            if (redemption == null || redemption.Reward == null || (redemption.Reward.FamilyId != null && redemption.Reward.FamilyId != user.FamilyId)) return null;
            if (user.Role == UserRole.Child && redemption.ChildUserId != userId) return null;

            return MapToDto(redemption);
        }

        public async Task<RewardRedemptionDto?> RedeemRewardAsync(int rewardId, int childUserId)
        {
            var child = await _db.Users.FindAsync(childUserId);
            if (child == null || child.FamilyId == null || child.Role != UserRole.Child) return null;

            var reward = await _db.Rewards.FindAsync(rewardId);
            if (reward == null || (reward.FamilyId != null && reward.FamilyId != child.FamilyId)) return null;

            if (child.Points < reward.RequiredPoints)
            {
                throw new InvalidOperationException($"Số điểm hiện tại của con ({child.Points} điểm) không đủ để đổi phần thưởng này ({reward.RequiredPoints} điểm).");
            }

            // Deduct child points immediately (hold point logic)
            child.Points -= reward.RequiredPoints;

            var redemption = new RewardRedemption
            {
                RewardId = rewardId,
                ChildUserId = childUserId,
                Status = RedemptionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.RewardRedemptions.Add(redemption);
            await _db.SaveChangesAsync();

            return await GetRedemptionByIdAsync(redemption.Id, childUserId);
        }

        public async Task<RewardRedemptionDto?> ApproveRedemptionAsync(int redemptionId, ApproveRedemptionDto dto, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            if (parent == null || parent.FamilyId == null || parent.Role != UserRole.Parent) return null;

            var redemption = await _db.RewardRedemptions
                .Include(rr => rr.Reward)
                .Include(rr => rr.Child)
                .FirstOrDefaultAsync(rr => rr.Id == redemptionId);

            if (redemption == null || redemption.Reward == null || (redemption.Reward.FamilyId != null && redemption.Reward.FamilyId != parent.FamilyId)) return null;
            if (redemption.Status != RedemptionStatus.Pending) return null; // Can only approve/reject pending redemptions

            redemption.Status = dto.Approved ? RedemptionStatus.Approved : RedemptionStatus.Rejected;
            redemption.ParentNote = dto.ParentNote;
            redemption.UpdatedAt = DateTime.UtcNow;

            // If rejected, refund points to child
            if (!dto.Approved && redemption.Child != null)
            {
                redemption.Child.Points += redemption.Reward.RequiredPoints;
            }

            await _db.SaveChangesAsync();

            return await GetRedemptionByIdAsync(redemptionId, parentUserId);
        }

        // ==================== MAPPERS ====================

        public static RewardDto MapToDto(Reward r) => new()
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            RequiredPoints = r.RequiredPoints,
            CreatedAt = r.CreatedAt,
            CreatedBy = r.CreatedBy != null ? AuthService.MapToDto(r.CreatedBy) : null
        };

        public static RewardRedemptionDto MapToDto(RewardRedemption rr) => new()
        {
            Id = rr.Id,
            RewardId = rr.RewardId,
            RewardTitle = rr.Reward?.Title ?? string.Empty,
            RequiredPoints = rr.Reward?.RequiredPoints ?? 0,
            RewardDescription = rr.Reward?.Description,
            Child = rr.Child != null ? AuthService.MapToDto(rr.Child) : null,
            Status = rr.Status.ToString(),
            ParentNote = rr.ParentNote,
            CreatedAt = rr.CreatedAt,
            UpdatedAt = rr.UpdatedAt
        };
    }
}
