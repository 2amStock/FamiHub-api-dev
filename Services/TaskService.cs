using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskDto>> GetTasksAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return new List<TaskDto>();

            IQueryable<FamilyTask> query = _db.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Proof).ThenInclude(p => p!.Child)
                .Where(t => t.FamilyId == user.FamilyId);

            // Child can only see their own tasks
            if (user.Role == UserRole.Child)
                query = query.Where(t => t.AssignedToUserId == userId);

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            var task = await _db.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Proof).ThenInclude(p => p!.Child)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || user == null || task.FamilyId != user.FamilyId) return null;
            if (user.Role == UserRole.Child && task.AssignedToUserId != userId) return null;

            return MapToDto(task);
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto dto, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            if (parent == null || parent.FamilyId == null) return null;

            if (dto.AssignedToUserId.HasValue)
            {
                var task = new FamilyTask
                {
                    FamilyId = parent.FamilyId.Value,
                    CreatedByUserId = parentUserId,
                    AssignedToUserId = dto.AssignedToUserId,
                    Title = dto.Title,
                    Description = dto.Description,
                    DueDate = dto.DueDate,
                    Points = dto.Points,
                    Status = Models.TaskStatus.Pending
                };

                _db.Tasks.Add(task);
                await _db.SaveChangesAsync();
                return await GetTaskByIdAsync(task.Id, parentUserId);
            }
            else
            {
                // Assign to ALL children in the family
                var children = await _db.Users
                    .Where(u => u.FamilyId == parent.FamilyId && u.Role == UserRole.Child)
                    .ToListAsync();

                if (!children.Any()) return null;

                FamilyTask? firstTask = null;
                foreach (var child in children)
                {
                    var task = new FamilyTask
                    {
                        FamilyId = parent.FamilyId.Value,
                        CreatedByUserId = parentUserId,
                        AssignedToUserId = child.Id,
                        Title = dto.Title,
                        Description = dto.Description,
                        DueDate = dto.DueDate,
                        Points = dto.Points,
                        Status = Models.TaskStatus.Pending
                    };
                    _db.Tasks.Add(task);
                    if (firstTask == null) firstTask = task;
                }

                await _db.SaveChangesAsync();
                return firstTask != null ? await GetTaskByIdAsync(firstTask.Id, parentUserId) : null;
            }
        }

        public async Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int parentUserId)
        {
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null) return null;

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.AssignedToUserId.HasValue) task.AssignedToUserId = dto.AssignedToUserId;
            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
            if (dto.Points.HasValue) task.Points = dto.Points.Value;
            task.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetTaskByIdAsync(taskId, parentUserId);
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int parentUserId)
        {
            var user = await _db.Users.FindAsync(parentUserId);
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null || user == null || task.FamilyId != user.FamilyId) return false;

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TaskDto?> SubmitTaskAsync(int taskId, int childUserId, string? note, string photoUrl)
        {
            var task = await _db.Tasks
                .Include(t => t.Proof)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.AssignedToUserId != childUserId) return null;
            if (task.Status != Models.TaskStatus.Pending && task.Status != Models.TaskStatus.InProgress && task.Status != Models.TaskStatus.Rejected)
                return null;

            // Remove old proof if exists
            if (task.Proof != null) _db.TaskProofs.Remove(task.Proof);

            var proof = new TaskProof
            {
                TaskId = taskId,
                ChildUserId = childUserId,
                PhotoUrl = photoUrl,
                Note = note,
                SubmittedAt = DateTime.UtcNow
            };

            task.Status = Models.TaskStatus.Submitted;
            task.UpdatedAt = DateTime.UtcNow;
            _db.TaskProofs.Add(proof);
            await _db.SaveChangesAsync();

            return await GetTaskByIdAsync(taskId, childUserId);
        }

        public async Task<TaskDto?> ApproveTaskAsync(int taskId, ApproveTaskDto dto, int parentUserId)
        {
            var parent = await _db.Users.FindAsync(parentUserId);
            var task = await _db.Tasks
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || parent == null || task.FamilyId != parent.FamilyId) return null;
            if (task.Status != Models.TaskStatus.Submitted) return null;

            task.Status = dto.Approved ? Models.TaskStatus.Approved : Models.TaskStatus.Rejected;
            task.RejectionNote = dto.Approved ? null : dto.RejectionNote;
            task.UpdatedAt = DateTime.UtcNow;

            if (dto.Approved && task.AssignedTo != null)
            {
                task.AssignedTo.Points += task.Points;
            }

            await _db.SaveChangesAsync();
            return await GetTaskByIdAsync(taskId, parentUserId);
        }

        public static TaskDto MapToDto(FamilyTask t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Points = t.Points,
            Status = t.Status.ToString(),
            DueDate = t.DueDate,
            CreatedAt = t.CreatedAt,
            RejectionNote = t.RejectionNote,
            AssignedTo = t.AssignedTo != null ? AuthService.MapToDto(t.AssignedTo) : null,
            CreatedBy = t.CreatedBy != null ? AuthService.MapToDto(t.CreatedBy) : null,
            Proof = t.Proof != null ? new TaskProofDto
            {
                Id = t.Proof.Id,
                PhotoUrl = t.Proof.PhotoUrl,
                Note = t.Proof.Note,
                SubmittedAt = t.Proof.SubmittedAt,
                Child = t.Proof.Child != null ? AuthService.MapToDto(t.Proof.Child) : null
            } : null
        };
    }
}
