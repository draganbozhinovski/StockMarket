using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class StockSymbolsPriceGrain : GrainBase, IStockSymbolsPriceGrain
    {
        private string _price = null!;
        public override async Task OnActivateAsync()
        {
            string allStocks;
            this.GetPrimaryKey(out allStocks);
            var url = new Uri("https://localhost:7015/notificationhub");

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            await hubConnection.StartAsync();

            await UpdatePrice(allStocks);
            var timer = RegisterTimer(
                                        UpdatePrice,
                                        allStocks,
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(1));

            await base.OnActivateAsync();
        }

        private async Task UpdatePrice(object allStocks)
        {
            Console.WriteLine($"Grain -> {(string)allStocks}");
            List<string> stocks = new List<string> { "BTC", "ETH", "DOT", "ADA", "SOL", "DOGE", "MATIC" };
            List<PriceUpdate> allRates = new List<PriceUpdate>();
            foreach (var stock in stocks)
            {
                _price = await GetPriceQuote(stock);
                allRates.Add(JsonConvert.DeserializeObject<PriceUpdate>(_price));
                Console.WriteLine($"Price for: {stock} -> {_price}");
            }
            var dataRates = JsonConvert.SerializeObject(allRates);
            await SendAllRates(dataRates);

        }

        private async Task<string> GetPriceQuote(string stock)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{stock}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }

        public async Task SendAllRates(string message)
        {
            await hubConnection.SendAsync("AllRates", message);

        }

        public Task<string> GetSymbolsPrice() => Task.FromResult(_price);
    }
}
