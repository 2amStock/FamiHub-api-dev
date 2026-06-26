using System.Security.Claims;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Hubs;
using FamiHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/shoppinglists")]
    [Authorize]
    public class ShoppingListsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ShoppingHub> _hubContext;

        public ShoppingListsController(AppDbContext context, IHubContext<ShoppingHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private int? GetFamilyId()
        {
            var familyIdClaim = User.FindFirstValue("FamilyId");
            if (string.IsNullOrEmpty(familyIdClaim)) return null;
            return int.TryParse(familyIdClaim, out var id) ? id : null;
        }

        private async Task<bool> CheckSubscriptionAccessAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var currentPlan = await _context.SubscriptionPlans.FindAsync(user.CurrentPlanId);
            return currentPlan != null && currentPlan.HasShoppingList;
        }

        private async Task<ShoppingList> GetOrCreateActiveListAsync(int familyId)
        {
            var activeList = await _context.ShoppingLists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.FamilyId == familyId && l.Status == "active");

            if (activeList == null)
            {
                activeList = new ShoppingList
                {
                    FamilyId = familyId,
                    Name = "Danh sách tuần này",
                    Status = "active"
                };
                _context.ShoppingLists.Add(activeList);
                await _context.SaveChangesAsync();
            }

            return activeList;
        }

        /// <summary>
        /// Lấy danh sách mua sắm hiện tại
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveList()
        {
            var userId = GetUserId();
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            if (!await CheckSubscriptionAccessAsync(userId))
                return StatusCode(403, new { message = "Gói cước hiện tại của bạn không hỗ trợ tính năng Shopping List. Vui lòng nâng cấp." });

            var list = await GetOrCreateActiveListAsync(familyId.Value);

            var dto = new ShoppingListDto
            {
                Id = list.Id,
                FamilyId = list.FamilyId,
                Name = list.Name,
                Status = list.Status,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items.Select(i => new ShoppingItemDto
                {
                    Id = i.Id,
                    ListId = i.ListId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    IsBought = i.IsBought,
                    BuyerId = i.BuyerId,
                    CreatedByUserId = i.CreatedByUserId,
                    CreatedAt = i.CreatedAt
                }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Thêm món đồ cần mua
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] CreateShoppingItemDto request)
        {
            var userId = GetUserId();
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            if (!await CheckSubscriptionAccessAsync(userId))
                return StatusCode(403, new { message = "Gói cước hiện tại của bạn không hỗ trợ." });

            var list = await GetOrCreateActiveListAsync(familyId.Value);

            var item = new ShoppingItem
            {
                ListId = list.Id,
                Name = request.Name,
                Quantity = request.Quantity,
                Unit = request.Unit,
                CreatedByUserId = userId
            };

            _context.ShoppingItems.Add(item);
            
            list.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
            await _context.SaveChangesAsync();

            var itemDto = new ShoppingItemDto
            {
                Id = item.Id,
                ListId = item.ListId,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                IsBought = item.IsBought,
                BuyerId = item.BuyerId,
                CreatedByUserId = item.CreatedByUserId,
                CreatedAt = item.CreatedAt
            };

            // Notify clients
            await _hubContext.Groups.ClientGroup($"Family_{familyId.Value}").SendAsync("ShoppingItemAdded", itemDto);

            return Ok(itemDto);
        }

        /// <summary>
        /// Sửa món đồ hoặc đánh dấu đã mua
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateShoppingItemDto request)
        {
            var userId = GetUserId();
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var item = await _context.ShoppingItems
                .Include(i => i.ShoppingList)
                .FirstOrDefaultAsync(i => i.Id == id && i.ShoppingList!.FamilyId == familyId.Value);

            if (item == null) return NotFound();

            if (request.Name != null) item.Name = request.Name;
            if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
            if (request.Unit != null) item.Unit = request.Unit;
            if (request.IsBought.HasValue)
            {
                item.IsBought = request.IsBought.Value;
                item.BuyerId = item.IsBought ? userId : null;
            }

            item.ShoppingList!.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
            await _context.SaveChangesAsync();

            var itemDto = new ShoppingItemDto
            {
                Id = item.Id,
                ListId = item.ListId,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                IsBought = item.IsBought,
                BuyerId = item.BuyerId,
                CreatedByUserId = item.CreatedByUserId,
                CreatedAt = item.CreatedAt
            };

            await _hubContext.Groups.ClientGroup($"Family_{familyId.Value}").SendAsync("ShoppingItemUpdated", itemDto);

            return Ok(itemDto);
        }

        /// <summary>
        /// Xóa món đồ
        /// </summary>
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var item = await _context.ShoppingItems
                .Include(i => i.ShoppingList)
                .FirstOrDefaultAsync(i => i.Id == id && i.ShoppingList!.FamilyId == familyId.Value);

            if (item == null) return NotFound();

            _context.ShoppingItems.Remove(item);
            item.ShoppingList!.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
            await _context.SaveChangesAsync();

            await _hubContext.Groups.ClientGroup($"Family_{familyId.Value}").SendAsync("ShoppingItemDeleted", id);

            return Ok(new { message = "Đã xóa." });
        }

        /// <summary>
        /// Chốt danh sách (Archive)
        /// </summary>
        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveList()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Parent")
                return StatusCode(403, new { message = "Chỉ phụ huynh mới có quyền chốt danh sách." });

            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var activeList = await _context.ShoppingLists
                .FirstOrDefaultAsync(l => l.FamilyId == familyId.Value && l.Status == "active");

            if (activeList != null)
            {
                activeList.Status = "archived";
                activeList.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
                await _context.SaveChangesAsync();
            }

            await _hubContext.Groups.ClientGroup($"Family_{familyId.Value}").SendAsync("ShoppingListArchived");

            return Ok(new { message = "Đã chốt danh sách mua sắm." });
        }

        /// <summary>
        /// Xem lịch sử
        /// </summary>
        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedLists()
        {
            var familyId = GetFamilyId();
            if (familyId == null) return BadRequest(new { message = "Bạn cần tham gia một gia đình trước." });

            var lists = await _context.ShoppingLists
                .Include(l => l.Items)
                .Where(l => l.FamilyId == familyId.Value && l.Status == "archived")
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var result = lists.Select(list => new ShoppingListDto
            {
                Id = list.Id,
                FamilyId = list.FamilyId,
                Name = list.Name,
                Status = list.Status,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items.Select(i => new ShoppingItemDto
                {
                    Id = i.Id,
                    ListId = i.ListId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    IsBought = i.IsBought,
                    BuyerId = i.BuyerId,
                    CreatedByUserId = i.CreatedByUserId,
                    CreatedAt = i.CreatedAt
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}
