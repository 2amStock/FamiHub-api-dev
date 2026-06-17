using FamiHub.API.Data;
using FamiHub.API.Models;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace FamiHub.API.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _db;
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext db, IConfiguration config, ILogger<PaymentService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
            var options = new PayOSOptions
            {
                ClientId = _config["PayOS:ClientId"] ?? "",
                ApiKey = _config["PayOS:ApiKey"] ?? "",
                ChecksumKey = _config["PayOS:ChecksumKey"] ?? ""
            };
            _payOS = new PayOSClient(options);
        }

        public async Task<string?> CreatePaymentLinkAsync(int userId, int planId, string? returnUrl = null)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy User trong Database.");

            var plan = await _db.SubscriptionPlans.FindAsync(planId);
            if (plan == null) throw new Exception($"Không tìm thấy Gói cước (PlanId = {planId}) trong Database. Có thể bạn chưa chạy Migration nạp dữ liệu Gói cước.");

            var orderCode = long.Parse(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().Substring(0, 10));

            var transaction = new FamiHub.API.Models.PaymentTransaction
            {
                UserId = userId,
                SubscriptionPlanId = planId,
                OrderCode = orderCode.ToString(),
                Amount = plan.Price,
                Status = "PENDING"
            };

            _db.PaymentTransactions.Add(transaction);
            await _db.SaveChangesAsync();

            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)plan.Price,
                Description = $"FamiHub {plan.Name}",
                CancelUrl = returnUrl ?? "https://famihub.com/cancel",
                ReturnUrl = returnUrl ?? "https://famihub.com/success",
                Items = [new PaymentLinkItem { Name = plan.Name, Quantity = 1, Price = (int)plan.Price }]
            };

            try
            {
                var response = await _payOS.PaymentRequests.CreateAsync(paymentData);
                return response.CheckoutUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayOS payment link");
                throw;
            }
        }

        public async Task<bool> ProcessWebhookAsync(Webhook webhookBody)
        {
            try
            {
                var webhookData = await _payOS.Webhooks.VerifyAsync(webhookBody);
                if (webhookData.Code == "00")
                {
                    var transaction = await _db.PaymentTransactions
                        .FirstOrDefaultAsync(t => t.OrderCode == webhookData.OrderCode.ToString());

                    if (transaction != null && transaction.Status == "PENDING")
                    {
                        transaction.Status = "SUCCESS";
                        transaction.CompletedAt = FamiHub.API.Utils.AppTime.Now;

                        var userSub = new UserSubscription
                        {
                            UserId = transaction.UserId,
                            SubscriptionPlanId = transaction.SubscriptionPlanId,
                            StartDate = FamiHub.API.Utils.AppTime.Now,
                            EndDate = FamiHub.API.Utils.AppTime.Now.AddMonths(1),
                            Status = "ACTIVE",
                            PayOSOrderCode = webhookData.OrderCode.ToString(),
                            TransactionId = webhookData.Reference ?? ""
                        };

                        _db.UserSubscriptions.Add(userSub);

                        // Update the user directly
                        var user = await _db.Users.FindAsync(transaction.UserId);
                        if (user != null)
                        {
                            user.CurrentPlanId = transaction.SubscriptionPlanId;
                            user.SubscriptionExpiryTime = userSub.EndDate;
                            _db.Users.Update(user);
                        }

                        await _db.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process PayOS webhook");
            }
            return false;
        }

        public async Task<bool> IsUserPremiumAsync(int userId)
        {
            var activeSub = await _db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE" && s.EndDate > FamiHub.API.Utils.AppTime.Now);
            return activeSub != null;
        }

        public async Task<SubscriptionPlan> GetCurrentPlanAsync(int userId)
        {
            var activeSub = await _db.UserSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE" && s.EndDate > FamiHub.API.Utils.AppTime.Now);

            if (activeSub != null && activeSub.Plan != null)
            {
                return activeSub.Plan;
            }

            return await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Price == 0)
                   ?? new SubscriptionPlan { Name = "Free", Price = 0, DurationType = "NONE", MaxMembers = 3, MaxTasksPerDay = 5, HasAI = false };
        }
    }
}
