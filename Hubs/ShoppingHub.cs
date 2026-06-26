using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FamiHub.API.Hubs
{
    public class ShoppingHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var familyIdClaim = Context.User?.FindFirst("FamilyId");
            if (familyIdClaim != null)
            {
                // Thêm user vào group của Family để nhận event
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Family_{familyIdClaim.Value}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var familyIdClaim = Context.User?.FindFirst("FamilyId");
            if (familyIdClaim != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Family_{familyIdClaim.Value}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
