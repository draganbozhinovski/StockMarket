using Newtonsoft.Json;
using Orleans;
using SignalR.Orleans.Core;
using StockMarket.Common;

namespace StockMarket.SymbolService
{
    public class StockSymbolsPriceGrain : Grain, IStockSymbolsPriceGrain
    {
        private const string StockEndpoint = "https://api.coinbase.com/v2/prices/";
        private readonly HttpClient _httpClient = new();

        private string _price = null!;

        private HubContext<INotificationHub> _hubContext;

        public override async Task OnActivateAsync()
        {
            _hubContext = GrainFactory.GetHub<INotificationHub>();
            string allStocks;
            this.GetPrimaryKey(out allStocks);
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
            await SendMessageAsync(dataRates);

        }

        private async Task<string> GetPriceQuote(string stock)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{stock}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }

        private async Task SendMessageAsync(string message)
        {
            var groupId = this.GetPrimaryKeyString();



            await _hubContext.Group(groupId).Send("all-rates", message);
        }

        public Task<string> GetSymbolsPrice() => Task.FromResult(_price);
    }
}
