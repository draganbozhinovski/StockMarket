using Microsoft.AspNetCore.SignalR;
using Orleans;
using StockMarket.Common;
using System.Reflection;
using System.Threading.Channels;
using System.Xml.Linq;

namespace StockMarket.API.Hubs
{
    public class NotificationHub : Hub
    {
        public static Dictionary<string, User> connectedUsers = new Dictionary<string, User>();
        public NotificationHub()
        {
        }

        public async Task StreamAllRates(string streamName, IAsyncEnumerable<PriceUpdate> stream)
        {
            Console.WriteLine($"Receive stream {streamName}");
            await foreach (var item in stream)
            {
                //_logger.LogTrace($"received {item}");
            }
        }

        public async Task AllRates(List<PriceUpdate> priceUpdates)
        {
            await Clients.Group("all-rates").SendAsync("SendAllRates", priceUpdates);
        }

        public async Task RegisterUser(User user)
        {
            connectedUsers.Add(Context.ConnectionId, user);
        }

        public async Task NotifyOrder(string method, OrderInProcess orderInProcess)
        {
            if (connectedUsers.Any(x => x.Value.Id == orderInProcess.User.Id))
            {
                var connectedUser = connectedUsers.FirstOrDefault(x => x.Value.Id == orderInProcess.User.Id);
                await Clients.Client(connectedUser.Key).SendAsync(method, orderInProcess);
            }
        }

        public async Task NotifyWallet(List<WalletCurrency> walletCurrencies, User user)
        {
            if (connectedUsers.Any(x => x.Value.Id == user.Id))
            {
                var connectedUser = connectedUsers.FirstOrDefault(x => x.Value.Id == user.Id);
                await Clients.Client(connectedUser.Key).SendAsync("NotifyWallet", walletCurrencies);
            }
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
