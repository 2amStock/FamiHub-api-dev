using FamiHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Security.Claims;

namespace FamiHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [Authorize]
        [HttpPost("create-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var userId = GetUserId();
                var url = await _paymentService.CreatePaymentLinkAsync(userId, dto.PlanId, dto.ReturnUrl);

                if (url == null)
                {
                    return BadRequest(new { message = "Không thể tạo link thanh toán." });
                }

                return Ok(new { checkoutUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi PayOS: {ex.Message}" });
            }
        }

        // Endpoint trung gian để tự động Deep Link về App Flutter sau khi thanh toán qua trình duyệt
        [HttpGet("redirect-app")]
        public IActionResult RedirectToApp()
        {
            var html = @"
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1'>
                <title>Thanh toán thành công</title>
                <style>
                    body { font-family: sans-serif; text-align: center; padding-top: 50px; }
                    .btn { display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; margin-top: 20px; }
                </style>
            </head>
            <body>
                <h2>Thanh toán thành công!</h2>
                <p>Đang chuyển hướng về FamiHub...</p>
                <p>Nếu ứng dụng không tự mở, hãy bấm nút bên dưới:</p>
                <a href='famihub://payment-success' class='btn'>Quay lại App</a>
                <script>
                    setTimeout(function() {
                        window.location.href = 'famihub://payment-success';
                    }, 500);
                </script>
            </body>
            </html>";
            
            return Content(html, "text/html; charset=utf-8");
        }

        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook webhookBody)
        {
            try
            {
                var result = await _paymentService.ProcessWebhookAsync(webhookBody);
                // Luôn trả về 200 OK để PayOS xác nhận Webhook thành công 
                // (kể cả khi là webhook test không có transaction thật)
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CreatePaymentDto
    {
        public int PlanId { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
