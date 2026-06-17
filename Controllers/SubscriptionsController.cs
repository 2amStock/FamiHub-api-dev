using System.Security.Claims;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();
            var dtos = plans.Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationType = p.DurationType,
                MaxMembers = p.MaxMembers,
                MaxTasksPerDay = p.MaxTasksPerDay,
                HasAI = p.HasAI,
                HasCalendar = p.HasCalendar,
                HasShoppingList = p.HasShoppingList,
                HasStudyTracking = p.HasStudyTracking,
                HasAchievement = p.HasAchievement
            });
            return Ok(dtos);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSubscription()
        {
            var userId = GetUserId();
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");

            if (subscription == null || subscription.Plan == null)
            {
                // Fallback to FREE plan if no active subscription
                var freePlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "FREE");
                if (freePlan == null) return NotFound("FREE plan not found");

                return Ok(new UserSubscriptionDto
                {
                    UserId = userId,
                    Plan = new SubscriptionPlanDto
                    {
                        Id = freePlan.Id,
                        Name = freePlan.Name,
                        Price = freePlan.Price,
                        DurationType = freePlan.DurationType,
                        MaxMembers = freePlan.MaxMembers,
                        MaxTasksPerDay = freePlan.MaxTasksPerDay,
                        HasAI = freePlan.HasAI,
                        HasCalendar = freePlan.HasCalendar,
                        HasShoppingList = freePlan.HasShoppingList,
                        HasStudyTracking = freePlan.HasStudyTracking,
                        HasAchievement = freePlan.HasAchievement
                    },
                    StartDate = FamiHub.API.Utils.AppTime.Now,
                    EndDate = FamiHub.API.Utils.AppTime.Now.AddYears(100),
                    Status = "ACTIVE"
                });
            }

            return Ok(new UserSubscriptionDto
            {
                UserId = subscription.UserId,
                Plan = new SubscriptionPlanDto
                {
                    Id = subscription.Plan.Id,
                    Name = subscription.Plan.Name,
                    Price = subscription.Plan.Price,
                    DurationType = subscription.Plan.DurationType,
                    MaxMembers = subscription.Plan.MaxMembers,
                    MaxTasksPerDay = subscription.Plan.MaxTasksPerDay,
                    HasAI = subscription.Plan.HasAI,
                    HasCalendar = subscription.Plan.HasCalendar,
                    HasShoppingList = subscription.Plan.HasShoppingList,
                    HasStudyTracking = subscription.Plan.HasStudyTracking,
                    HasAchievement = subscription.Plan.HasAchievement
                },
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status
            });
        }
    }
}
