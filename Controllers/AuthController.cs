using System.Security.Claims;
using FamiHub.API.DTOs;
using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(AuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin." });

            var result = await _authService.RegisterAsync(dto);
            if (result == null)
                return Conflict(new { message = "Email đã được sử dụng." });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                if (result == null)
                    return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

                return Ok(result);
            }
            catch (Exception ex) when (ex.Message == "unverified_email")
            {
                return StatusCode(403, new { message = "Vui lòng xác thực email trước khi đăng nhập.", errorCode = "UNVERIFIED_EMAIL" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var isVerified = await _authService.VerifyOtpAsync(dto);
            if (!isVerified)
                return BadRequest(new { message = "Mã xác thực không hợp lệ hoặc đã hết hạn." });

            return Ok(new { message = "Xác thực email thành công. Bạn có thể đăng nhập ngay." });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            var success = await _authService.ResendOtpAsync(dto);
            if (!success)
                return BadRequest(new { message = "Không thể gửi lại mã. Tài khoản không tồn tại hoặc đã được xác thực." });

            return Ok(new { message = "Mã xác thực mới đã được gửi đến email của bạn." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.GetUserByIdAsync(userId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                return BadRequest(new { message = "Vui lòng cung cấp email nhận (tham số 'to')." });

            try
            {
                await _emailService.SendEmailAsync(to, "Test Gửi Email từ FamiHub", "<h1>Thành công!</h1><p>Đây là email test gửi từ hệ thống FamiHub API.</p>");
                return Ok(new { message = $"Đã gửi email test thành công tới {to}!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi mail.", error = ex.Message });
            }
        }
        //hehehehe
    }
}
