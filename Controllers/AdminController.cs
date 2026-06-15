using Microsoft.AspNetCore.Mvc;
using FamiHub.API.Services;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public AdminController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public class SendTesterEmailRequest
        {
            public List<string> Emails { get; set; } = new List<string>();
        }

        [HttpPost("SendTesterEmail")]
        public async Task<IActionResult> SendTesterEmail([FromBody] SendTesterEmailRequest request)
        {
            if (request == null || request.Emails == null || !request.Emails.Any())
            {
                return BadRequest(new { message = "Danh sách email trống" });
            }

            var subject = "Thư mời trải nghiệm ứng dụng FamiHub";
            var htmlBody = @"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #2D3748; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #E2E8F0; border-radius: 12px;'>
                    <h2 style='color: #FF7EB3; border-bottom: 2px solid #F7FAFC; padding-bottom: 12px;'>Chào bạn,</h2>
                    
                    <p>Cảm ơn bạn đã quan tâm và đăng ký dùng thử <b>FamiHub</b>.</p>
                    <p>Chúng tôi đã cập nhật tài khoản của bạn vào danh sách trải nghiệm sớm trên hệ thống của Google Play.</p>
                    
                    <p>Để bắt đầu sử dụng, vui lòng thực hiện theo các bước sau:</p>
                    
                    <div style='background: #F7FAFC; padding: 20px; border-radius: 8px; margin: 24px 0;'>
                        <p style='margin: 0 0 12px 0;'><b>1.</b> Mở ứng dụng <b>CH Play (Google Play Store)</b> trên điện thoại.</p>
                        <p style='margin: 0 0 12px 0;'><b>2.</b> Tìm kiếm chính xác từ khóa: <b><span style='color: #E53E3E; font-size: 16px;'>com.famihub.app</span></b></p>
                        <p style='margin: 0;'><b>3.</b> Chọn ứng dụng FamiHub và nhấn <b>Cài đặt</b>.</p>
                    </div>

                    <p style='font-size: 14px; color: #4A5568;'><i>Lưu ý: Nếu không tìm thấy, vui lòng xác nhận quyền trải nghiệm trước qua liên kết bảo mật từ Google Play:</i><br>
                    <a href='https://play.google.com/apps/testing/com.famihub.app' style='color: #3182CE; text-decoration: none; font-weight: 500;'>👉 Mở trang xác nhận quyền tải ứng dụng</a></p>

                    <p style='margin-top: 24px;'>Nếu cần hỗ trợ, bạn chỉ cần phản hồi lại email này.</p>
                    <p>Trân trọng,<br><b>Đội ngũ FamiHub</b></p>

                    <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #E2E8F0; color: #A0AEC0; font-size: 12px; text-align: center;'>
                        <p style='margin: 0;'>Email này được gửi vì bạn đã đăng ký nhận thông tin từ FamiHub.</p>
                        <p style='margin: 4px 0 0 0;'>Nếu đây là sự nhầm lẫn, vui lòng bỏ qua thư này hoặc phản hồi để chúng tôi xóa bạn khỏi danh sách.</p>
                    </div>
                </div>";

            int successCount = 0;
            List<string> failedEmails = new List<string>();

            foreach (var email in request.Emails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;
                
                try
                {
                    await _emailService.SendEmailAsync(email.Trim(), subject, htmlBody, isHtml: true);
                    successCount++;
                }
                catch
                {
                    failedEmails.Add(email);
                }
            }

            return Ok(new 
            { 
                message = $"Gửi thành công {successCount}/{request.Emails.Count} email.",
                failed = failedEmails
            });
        }
    }
}
