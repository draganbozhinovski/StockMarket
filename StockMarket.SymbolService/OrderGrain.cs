using Microsoft.AspNetCore.Components.RenderTree;
using Newtonsoft.Json;
using Orleans;
using SignalR.Orleans.Core;
using StockMarket.Common;

namespace StockMarket.SymbolService
{
    public class OrderGrain : Grain, IOrderGrain
    {
        private const string StockEndpoint = "https://api.coinbase.com/v2/prices/";
        private readonly HttpClient _httpClient = new();
        private bool _processStatus = true;

        private HubContext<INotificationHub> _hubContext;

        public override async Task OnActivateAsync()
        {
            _hubContext = GrainFactory.GetHub<INotificationHub>();
            await base.OnActivateAsync();
        }

        private async Task ProcessOrder(Order order)
        {
            while (_processStatus)
            {
                var price = await GetPriceQuote(order.Stock);
                var stockData = JsonConvert.DeserializeObject<PriceUpdate>(price);
                var message = $"Stock:{order.Stock} Bid:{order.Bid} Ammount:{stockData?.Data.Amount} Number:{order.Number}";
                Console.WriteLine($"Order for User {order.User} with Id {order.Id} -> {message}");
                _hubContext.Client(order.User).SendOneWay("order-execution", message);
                if(Convert.ToDouble(stockData?.Data.Amount) <= order.Bid)
                {
                    //Continue to inform the user success and update the cache balance
                    _processStatus = false;
                }
                Thread.Sleep(5000);
            }
        }

        private async Task<string> GetPriceQuote(string currency)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{currency}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }


        public Task CreateOrder(Order order)
        {
            return Task.FromResult(ProcessOrder(order)); ;
        }
    }
}
