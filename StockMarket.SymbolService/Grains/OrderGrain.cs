using Newtonsoft.Json;
using Orleans;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;


        public override async Task OnActivateAsync()
        {
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
                if (Convert.ToDouble(stockData?.Data.Amount) <= order.Bid)
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
