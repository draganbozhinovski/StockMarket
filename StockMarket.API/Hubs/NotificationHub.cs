using Microsoft.AspNetCore.SignalR;
using Orleans;
using StockMarket.Common;
using System.Reflection;
using System.Xml.Linq;

namespace StockMarket.API.Hubs
{
    public class NotificationHub : Hub, INotificationHub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly IClusterClient _client;

        public NotificationHub(ILogger<NotificationHub> logger, IClusterClient clusterClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = clusterClient ?? throw new ArgumentNullException(nameof(clusterClient));
        }

        public async Task SendMessage(string message)
        {
           await  Clients.Group("all-rates").SendAsync("SendAllRates", message);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"{nameof(OnConnectedAsync)} called.");

            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-rates");

            var userNotificationGrain = _client.GetGrain<IStockSymbolsPriceGrain>("all-rates");
            await userNotificationGrain.GetSymbolsPrice();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation(exception, $"{nameof(OnDisconnectedAsync)} called.");

            await base.OnDisconnectedAsync(exception);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-rates");
        }

        public Task SendAllRates(string message)
        {
            throw new NotImplementedException();
        }

        public Task SendMessageToGroup(string groupname, string sender, string message)
        {
            return Clients.Group(groupname).SendAsync("SendMessage", sender, message);
        }
    }
}
