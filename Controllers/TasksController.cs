using System.Security.Claims;
using FamiHub.API.DTOs;
using FamiHub.API.Models;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly FileService _fileService;

        public TasksController(TaskService taskService, FileService fileService)
        {
            _taskService = taskService;
            _fileService = fileService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "";

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetTasksAsync(GetUserId());
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id, GetUserId());
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Tiêu đề không được trống." });

            var task = await _taskService.CreateTaskAsync(dto, GetUserId());
            if (task == null) return BadRequest(new { message = "Không thể tạo task. Hãy đảm bảo bạn đã có gia đình." });

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            var task = await _taskService.UpdateTaskAsync(id, dto, GetUserId());
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            if (GetRole() != "Parent")
                return Forbid();

            var result = await _taskService.DeleteTaskAsync(id, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitTask(int id, [FromBody] SubmitTaskDto dto)
        {
            if (GetRole() != "Child")
                return Forbid();

            if (string.IsNullOrEmpty(dto.PhotoUrl))
                return BadRequest(new { message = "Vui lòng chụp ảnh bằng chứng." });

            var task = await _taskService.SubmitTaskAsync(id, GetUserId(), dto.Note, dto.PhotoUrl);
            if (task == null)
                return BadRequest(new { message = "Không thể submit task. Task có thể không thuộc về bạn hoặc đã được duyệt." });

            return Ok(task);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveTask(int id, [FromBody] ApproveTaskDto dto)
        {
            if (GetRole() != "Parent")
                return Forbid();

            var task = await _taskService.ApproveTaskAsync(id, dto, GetUserId());
            if (task == null)
                return BadRequest(new { message = "Không thể duyệt task. Task cần ở trạng thái Submitted." });

            return Ok(task);
        }
    }
}
