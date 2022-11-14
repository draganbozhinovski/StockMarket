using Microsoft.AspNetCore.SignalR.Client;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.HubClient
{
    public class Notifier : INotifier
    {
        public HubConnection hubConnection;
        public Uri _hubUrl = new Uri("https://localhost:7015/notificationhub");
        public Notifier()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();
        }
        public async Task Notify(string method, object message)
        {
            if (hubConnection.State != HubConnectionState.Disconnected)
            {
                await hubConnection.SendAsync(method, message);
            }
            else
            {
                await hubConnection.StopAsync();
                await ConnectToHub();
                await hubConnection.SendAsync(method, message);
            }
        }

        public async Task Notify(string method, object message1, object message2)
        {
            if (hubConnection.State != HubConnectionState.Disconnected)
            {
                await hubConnection.SendAsync(method, message1, message2);
            }
            else
            {
                await hubConnection.StopAsync();
                await ConnectToHub();
                await hubConnection.SendAsync(method, message1, message2);
            }
        }

        public async Task Notify(string method, string clientMethod, object message)
        {
            if (hubConnection.State != HubConnectionState.Disconnected)
            {
                await hubConnection.SendAsync(method, clientMethod, message);
            }
            else
            {
                await hubConnection.StopAsync();
                await ConnectToHub();
                await hubConnection.SendAsync(method, clientMethod, message);
            }
        }

        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();

            await hubConnection.StartAsync();
        }


    }
}
