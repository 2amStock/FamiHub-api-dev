using FamiHub.API.Data;
using FamiHub.API.Hubs;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamiHub.API.Services
{
    public class PushNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly AppDbContext _db;

        public PushNotificationService(IHubContext<NotificationHub> hubContext, AppDbContext db)
        {
            _hubContext = hubContext;
            _db = db;
        }

        public async Task SendNotificationAsync(int userId, string title, string body, string? fcmToken, object? data = null)
        {
            // 1. Send via SignalR (Real-time in-app)
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                title,
                body,
                data
            });

            // 2. Send via Firebase Cloud Messaging (Push Notification)
            if (!string.IsNullOrEmpty(fcmToken))
            {
                try
                {
                    var message = new Message()
                    {
                        Token = fcmToken,
                        Notification = new Notification()
                        {
                            Title = title,
                            Body = body
                        },
                        Data = data as Dictionary<string, string> ?? new Dictionary<string, string>()
                    };

                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FCM Error sending to user {userId}: {ex.Message}");
                }
            }
        }

        public async Task SendFamilyNotificationAsync(int familyId, string title, string message, int? excludeUserId = null)
        {
            var users = await _db.Users.Where(u => u.FamilyId == familyId && u.Id != excludeUserId).ToListAsync();
            foreach (var u in users)
            {
                await SendNotificationAsync(u.Id, title, message, u.FcmToken);
            }
        }

        public async Task SendFamilyRefreshAsync(int familyId, int? excludeUserId = null)
        {
            var users = await _db.Users.Where(u => u.FamilyId == familyId && u.Id != excludeUserId).ToListAsync();
            foreach (var u in users)
            {
                await _hubContext.Clients.User(u.Id.ToString()).SendAsync("RefreshData");
            }
        }
    }
}
