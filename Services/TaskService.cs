using FamiHub.API.Data;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FamiHub.API.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;
        private readonly PaymentService _paymentService;
        private readonly PushNotificationService _pushNotificationService;

        public TaskService(AppDbContext db, PaymentService paymentService, PushNotificationService pushNotificationService)
        {
            _db = db;
            _paymentService = paymentService;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<List<TaskDto>> GetTasksAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.FamilyId == null) return new List<TaskDto>();

            IQueryable<FamilyTask> query = _db.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Proof).ThenInclude(p => p!.Child)
                .Where(t => t.FamilyId == user.FamilyId)
                .Where(t => !(t.Status == Models.TaskStatus.Pending && t.DueDate != null && t.DueDate < FamiHub.API.Utils.AppTime.Now));

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

            // Check limits
            var currentPlan = await _paymentService.GetCurrentPlanAsync(parentUserId);
            var todayTasks = await _db.Tasks
                .CountAsync(t => t.FamilyId == parent.FamilyId && t.CreatedAt.Date == FamiHub.API.Utils.AppTime.Now.Date);
            if (todayTasks >= currentPlan.MaxTasksPerDay)
            {
                throw new Exception($"LIMIT_EXCEEDED:{currentPlan.MaxTasksPerDay}");
            }

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
                
                var notification = new Notification
                {
                    UserId = dto.AssignedToUserId.Value,
                    Title = "Nhiệm vụ mới",
                    Message = $"Ba mẹ vừa giao cho con nhiệm vụ: {task.Title}",
                    Type = "TASK",
                    RelatedId = task.Id
                };
                _db.Notifications.Add(notification);

                await _db.SaveChangesAsync();
                
                var childUser = await _db.Users.FindAsync(dto.AssignedToUserId.Value);
                if (childUser != null)
                {
                    await _pushNotificationService.SendNotificationAsync(childUser.Id, notification.Title, notification.Message, childUser.FcmToken);
                }

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

                    var notification = new Notification
                    {
                        UserId = child.Id,
                        Title = "Nhiệm vụ mới",
                        Message = $"Ba mẹ vừa giao cho con nhiệm vụ: {task.Title}",
                        Type = "TASK"
                    };
                    _db.Notifications.Add(notification);
                    
                    // We can't await SendNotification inside the foreach before SaveChanges if we don't want to slow it down, 
                    // but it's fine for small numbers
                    await _pushNotificationService.SendNotificationAsync(child.Id, notification.Title, notification.Message, child.FcmToken);
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
            task.UpdatedAt = FamiHub.API.Utils.AppTime.Now;

            await _db.SaveChangesAsync();
            await _pushNotificationService.SendFamilyRefreshAsync(task.FamilyId);
            return await GetTaskByIdAsync(taskId, parentUserId);
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int parentUserId)
        {
            var user = await _db.Users.FindAsync(parentUserId);
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null || user == null || task.FamilyId != user.FamilyId) return false;

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            await _pushNotificationService.SendFamilyRefreshAsync(task.FamilyId);
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
                SubmittedAt = FamiHub.API.Utils.AppTime.Now
            };

            task.Status = Models.TaskStatus.Submitted;
            task.UpdatedAt = FamiHub.API.Utils.AppTime.Now;
            _db.TaskProofs.Add(proof);

            var notification = new Notification
            {
                UserId = task.CreatedByUserId,
                Title = "Nộp minh chứng",
                Message = $"Bé vừa nộp minh chứng cho nhiệm vụ: {task.Title}. Hãy kiểm tra và duyệt nhé!",
                Type = "TASK",
                RelatedId = task.Id
            };
            _db.Notifications.Add(notification);

            await _db.SaveChangesAsync();

            var parentUser = await _db.Users.FindAsync(task.CreatedByUserId);
            if (parentUser != null)
            {
                await _pushNotificationService.SendNotificationAsync(parentUser.Id, notification.Title, notification.Message, parentUser.FcmToken);
            }

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
            task.UpdatedAt = FamiHub.API.Utils.AppTime.Now;

            if (dto.Approved && task.AssignedTo != null)
            {
                task.AssignedTo.Points += task.Points;
            }

            if (task.AssignedToUserId.HasValue)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedToUserId.Value,
                    Title = dto.Approved ? "Nhiệm vụ được duyệt" : "Nhiệm vụ bị từ chối",
                    Message = dto.Approved 
                        ? $"Tuyệt vời! Nhiệm vụ '{task.Title}' đã được duyệt. Con được cộng {task.Points} điểm." 
                        : $"Nhiệm vụ '{task.Title}' chưa đạt yêu cầu. Lời nhắn: {dto.RejectionNote}",
                    Type = "TASK",
                    RelatedId = task.Id
                };
                _db.Notifications.Add(notification);
                
                var childUser = await _db.Users.FindAsync(task.AssignedToUserId.Value);
                if (childUser != null)
                {
                    await _pushNotificationService.SendNotificationAsync(childUser.Id, notification.Title, notification.Message, childUser.FcmToken);
                }
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
