using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        [AllowAnonymous]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file để upload.");

            try
            {
                var url = await _fileService.SaveFileAsync(file);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi upload: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("UploadAsync")]
        public async Task<IActionResult> UploadAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file để upload.");

            try
            {
                var url = await _fileService.SaveFileAsync(file);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi upload (Cloudinary): {ex.Message}");
            }
        }
    }
}
