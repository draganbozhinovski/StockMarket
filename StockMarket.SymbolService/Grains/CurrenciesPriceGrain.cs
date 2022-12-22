using Newtonsoft.Json;
using Orleans.Concurrency;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.HubClient;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class CurrenciesPriceGrain : GrainBase, ICurrenciesPriceGrain
    {
        private readonly INotifier _notifier;
        public CurrenciesPriceGrain(INotifier notifier)
        {
            _notifier = notifier;
        }
        public override async Task OnActivateAsync()
        {
            RegisterTimer(async _ =>
            {
                await UpdatePrice();
                await Task.CompletedTask;
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            await base.OnActivateAsync();
        }

        public virtual new IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period) =>
            base.RegisterTimer(asyncCallback, state, dueTime, period);


        public Task StartSymbolsPrice() => Task.CompletedTask;

        private async Task UpdatePrice()
        {
            List<string> currencies = new Currencies().CurrenciesInUse;
            List<PriceUpdate> allRates = new List<PriceUpdate>();
            foreach (var stock in currencies)
            {
                allRates.Add(JsonConvert.DeserializeObject<PriceUpdate>(await GetPriceQuote(stock)));
            }
            await _notifier.Notify("AllRates", allRates);
        }

        private async Task<string> GetPriceQuote(string stock)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{stock}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }
    }
}
