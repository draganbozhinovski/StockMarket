using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class CurrenciesPriceGrain : GrainBase, ICurrenciesPriceGrain
    {
        private string _price = null!;
        public override async Task OnActivateAsync()
        {
            string allCurrencies;
            this.GetPrimaryKey(out allCurrencies);

            await ConnectToHub();

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
            if(hubConnection.State == HubConnectionState.Disconnected)
            {
                await ConnectToHub();
            }
            List<string> currencies = new Currencies().CurrenciesInUse;
            List<PriceUpdate> allRates = new List<PriceUpdate>();
            foreach (var stock in currencies)
            {
                _price = await GetPriceQuote(stock);
                allRates.Add(JsonConvert.DeserializeObject<PriceUpdate>(_price));
            }
            await SendAllRates(allRates);

        }

        private async Task<string> GetPriceQuote(string stock)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{stock}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }

        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();

            await hubConnection.StartAsync();
        }

        public async Task SendAllRates(List<PriceUpdate> priceUpdates)
        {
            await hubConnection.SendAsync("AllRates", priceUpdates);
        }

        public Task<string> GetSymbolsPrice() => Task.FromResult(_price);


    }
}
