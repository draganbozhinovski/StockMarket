using Microsoft.AspNetCore.SignalR;
using Orleans;
using StockMarket.Common;
using System.Reflection;
using System.Xml.Linq;

namespace StockMarket.API.Hubs
{
    public class NotificationHub : Hub
    {
        public NotificationHub()
        {
        }

        public async Task AllRates(string message)
        {
            await Clients.Group("all-rates").SendAsync("SendAllRates", message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-rates");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-rates");
        }

        public Task SendMessageToGroup(string groupname, string sender, string message)
        {
            return Clients.Group(groupname).SendAsync("SendMessage", sender, message);
        }

    }
}
