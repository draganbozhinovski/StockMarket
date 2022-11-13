using Microsoft.AspNetCore.SignalR.Client;

namespace StockMarket.SymbolService.Observers
{
    public class OrderObserver : IOrderObserver
    {
        public HubConnection hubConnection;
        public Uri _hubUrl = new Uri("https://localhost:7015/notificationhub");
        public OrderObserver()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();
        }
        public async Task NotifyAllUsers(string method, object message)
        {
            await Task.FromResult(0);
        }
    }
}
