using System.Security.Claims;
using System.Text.Json;
using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FoodPreferencesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodPreferencesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetPreference()
        {
            var userId = GetUserId();
            var preference = await _context.UserFoodPreferences
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (preference == null)
            {
                var user = await _context.Users.FindAsync(userId);
                return Ok(new FoodPreferenceDto
                {
                    UserId = userId,
                    UserName = user?.Name ?? ""
                });
            }

            var dto = new FoodPreferenceDto
            {
                UserId = preference.UserId,
                UserName = preference.User?.Name ?? ""
            };

            if (!string.IsNullOrEmpty(preference.FavoriteDishes))
                dto.FavoriteDishes = JsonSerializer.Deserialize<List<string>>(preference.FavoriteDishes) ?? new();
                
            if (!string.IsNullOrEmpty(preference.DislikedIngredients))
                dto.DislikedIngredients = JsonSerializer.Deserialize<List<string>>(preference.DislikedIngredients) ?? new();
                
            if (!string.IsNullOrEmpty(preference.DietaryRestrictions))
                dto.DietaryRestrictions = JsonSerializer.Deserialize<List<string>>(preference.DietaryRestrictions) ?? new();
                
            if (!string.IsNullOrEmpty(preference.CuisinePreferences))
                dto.CuisinePreferences = JsonSerializer.Deserialize<List<string>>(preference.CuisinePreferences) ?? new();

            return Ok(dto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePreference([FromBody] UpdateFoodPreferenceDto dto)
        {
            var userId = GetUserId();
            var preference = await _context.UserFoodPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                preference = new UserFoodPreference
                {
                    UserId = userId
                };
                _context.UserFoodPreferences.Add(preference);
            }

            if (dto.FavoriteDishes != null)
                preference.FavoriteDishes = JsonSerializer.Serialize(dto.FavoriteDishes);
                
            if (dto.DislikedIngredients != null)
                preference.DislikedIngredients = JsonSerializer.Serialize(dto.DislikedIngredients);
                
            if (dto.DietaryRestrictions != null)
                preference.DietaryRestrictions = JsonSerializer.Serialize(dto.DietaryRestrictions);
                
            if (dto.CuisinePreferences != null)
                preference.CuisinePreferences = JsonSerializer.Serialize(dto.CuisinePreferences);
                
            preference.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPreference();
        }
    }
}
