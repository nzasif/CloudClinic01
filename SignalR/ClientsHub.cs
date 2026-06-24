using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace CloudClinic.SignalR
{
    [Authorize]
    public class ClientsHub : Hub
    {
        public ClientsHub()
        {
        }

        public async Task Send(HubNotificationModel notification)
        {
            await Clients.Users(notification.ReceiversIds).SendAsync(notification.ClientMethodName, notification.Data);
            await Clients.Caller.SendAsync("invoked", notification);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
