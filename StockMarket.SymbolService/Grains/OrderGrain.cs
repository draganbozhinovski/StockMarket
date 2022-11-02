using Newtonsoft.Json;
using Orleans;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;
        private Order? _order;
        private IUserGrain? _userGrain;

        private async Task ProcessOrder(Order order)
        {
            while (_processStatus)
            {
                var price = await GetPriceQuote(order.Currency.ToString());
                var stockData = JsonConvert.DeserializeObject<PriceUpdate>(price);

                var message = $"Stock:{order.Currency} Bid:{order.Bid} Ammount:{stockData?.Data.Amount} Number:{order.NumberOf}";

                Console.WriteLine($"Order for User {order.User.Name} with Id {order.Id} -> {message}");
                if (Convert.ToDouble(stockData?.Data.Amount) <= order.Bid)
                {
                    //Continue to inform the user success and update the cache balance
                    var usdtToRemove = Convert.ToDouble(stockData?.Data.Amount) * order.NumberOf;
                    //minus for processing the order to the platform account with observable 
                    
                    await _userGrain.RemoveUsdt(usdtToRemove);
                    var currencyToAdd = new WalletCurrency
                    {
                        Ammount = order.NumberOf,
                        Currency = order.Currency
                    };
                    await _userGrain.AddToWallet(currencyToAdd);

                    //notify all users for the orpdr processed with observable

                    _processStatus = false;
                }
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
            _order = order;
            _userGrain = GrainFactory.GetGrain<IUserGrain>(_order.User.Id);
            return Task.FromResult(ProcessOrder(order)); ;
        }
    }
}
