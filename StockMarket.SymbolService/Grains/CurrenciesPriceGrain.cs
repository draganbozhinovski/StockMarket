using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.HubClient;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class CurrenciesPriceGrain : GrainBase, ICurrenciesPriceGrain
    {
        private string _price = null!;
        INotifier _notifier;
        public override async Task OnActivateAsync()
        {
            string allCurrencies;
            this.GetPrimaryKey(out allCurrencies);
            _notifier = new Notifier();

            await UpdatePrice(allCurrencies);
            var timer = RegisterTimer(
                                        UpdatePrice,
                                        allCurrencies,
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(1));

            await base.OnActivateAsync();
        }


        private async Task UpdatePrice(object allCurrencies)
        {
            List<string> currencies = new Currencies().CurrenciesInUse;
            List<PriceUpdate> allRates = new List<PriceUpdate>();
            foreach (var stock in currencies)
            {
                _price = await GetPriceQuote(stock);
                allRates.Add(JsonConvert.DeserializeObject<PriceUpdate>(_price));
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

        public Task<string> GetSymbolsPrice() => Task.FromResult(_price);


    }
}
