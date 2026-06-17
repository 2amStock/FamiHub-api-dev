using FamiHub.API.Hubs;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FamiHub.API.Services
{
    public class PushNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public PushNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
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
    }
}
