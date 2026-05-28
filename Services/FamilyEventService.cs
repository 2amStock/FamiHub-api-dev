using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Services
{
    public class FamilyEventService
    {
        private readonly AppDbContext _context;

        public FamilyEventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FamilyEventDto>> GetEventsAsync(int familyId)
        {
            var events = await _context.FamilyEvents
                .Where(e => e.FamilyId == familyId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();

            return events.Select(e => new FamilyEventDto
            {
                Id = e.Id,
                FamilyId = e.FamilyId,
                CreatedByUserId = e.CreatedByUserId,
                Title = e.Title,
                Description = e.Description,
                StartTime = e.StartTime,
                EndTime = e.EndTime
            }).ToList();
        }

        public async Task<FamilyEventDto?> CreateEventAsync(int userId, CreateFamilyEventDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return null;

            var newEvent = new FamilyEvent
            {
                FamilyId = user.FamilyId.Value,
                CreatedByUserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _context.FamilyEvents.Add(newEvent);
            await _context.SaveChangesAsync();

            return new FamilyEventDto
            {
                Id = newEvent.Id,
                FamilyId = newEvent.FamilyId,
                CreatedByUserId = newEvent.CreatedByUserId,
                Title = newEvent.Title,
                Description = newEvent.Description,
                StartTime = newEvent.StartTime,
                EndTime = newEvent.EndTime
            };
        }

        public async Task<bool> DeleteEventAsync(int id, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return false;

            var familyEvent = await _context.FamilyEvents.FirstOrDefaultAsync(e => e.Id == id && e.FamilyId == user.FamilyId);
            if (familyEvent == null) return false;

            _context.FamilyEvents.Remove(familyEvent);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
