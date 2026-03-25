namespace FamiHub.API.DTOs
{
    // Auth DTOs
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Child"; // Parent or Child
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? FamilyId { get; set; }
        public string? FamilyName { get; set; }
        public int Points { get; set; }
        public string? Avatar { get; set; }
    }

    // Family DTOs
    public class CreateFamilyDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class JoinFamilyDto
    {
        public string InviteCode { get; set; } = string.Empty;
    }

    public class FamilyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public List<UserDto> Members { get; set; } = new();
    }

    // Task DTOs
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int Points { get; set; } = 10;
    }

    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Points { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Points { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? AssignedTo { get; set; }
        public UserDto? CreatedBy { get; set; }
        public TaskProofDto? Proof { get; set; }
        public string? RejectionNote { get; set; }
    }

    public class ApproveTaskDto
    {
        public bool Approved { get; set; }
        public string? RejectionNote { get; set; }
    }

    public class SubmitTaskDto
    {
        public string? Note { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    // Proof DTOs
    public class TaskProofDto
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime SubmittedAt { get; set; }
        public UserDto? Child { get; set; }
    }
}
