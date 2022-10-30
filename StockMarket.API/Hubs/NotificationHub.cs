using Microsoft.AspNetCore.SignalR;
using Orleans;
using StockMarket.Common;

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

        public async Task Send(string name, string message)
        {
            _logger.LogInformation($"{nameof(Send)} called. ConnectionId:{Context.ConnectionId}, Name:{name}, Message:{message}");

            var userNotificationGrain = _client.GetGrain<IStockSymbolsPriceGrain>(Guid.Empty.ToString());
            await userNotificationGrain.GetSymbolsPrice();
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"{nameof(OnConnectedAsync)} called.");

            await base.OnConnectedAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, Guid.Empty.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation(exception, $"{nameof(OnDisconnectedAsync)} called.");

            await base.OnDisconnectedAsync(exception);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Guid.Empty.ToString());
        }
    }
}
