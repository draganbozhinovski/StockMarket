using Binance.Client.Websocket.Client;
using Binance.Client.Websocket.Subscriptions;
using Binance.Client.Websocket.Websockets;
using Binance.Client.Websocket;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class CurrenciesPriceGrain : GrainBase, ICurrenciesPriceGrain
    {
        private Uri _url = new Uri("https://localhost:7015/notificationhub");
        private string _price = null!;
        public override async Task OnActivateAsync()
        {
            string allCurrencies;
            this.GetPrimaryKey(out allCurrencies);

            await ConnectToHub();

            await UpdateBinancePrice(allCurrencies);
            var timer = RegisterTimer(
                                        UpdateBinancePrice,
                                        allCurrencies,
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(1));

            await base.OnActivateAsync();
        }


        private async Task UpdateBinancePrice(object allCurrencies)
        {
            var url = BinanceValues.ApiWebsocketUrl;
            var exitEvent = new ManualResetEvent(false);
            using (var communicator = new BinanceWebsocketCommunicator(url))
            {
                using (var client = new BinanceWebsocketClient(communicator))
                {
                    client.Streams.TradesStream.Subscribe(async response =>
                    {

                        var trade = response.Data;
                        var priceUpdate = new PriceUpdate
                        {
                            Data = new Data
                            {
                                Amount = trade.Price,
                                Base = trade.Symbol.Replace("USDT", ""),
                                Currency = "USDT"
                            }
                        };
                        await SendAllRates(priceUpdate);
                    });

                    List<SubscriptionBase> subscriptions = new List<SubscriptionBase>();
                    List<string> currencies = new Currencies().CurrenciesInUse;
                    foreach (var currency in currencies)
                    {
                        subscriptions.Add(new TradeSubscription($"{currency.ToLower()}usdt"));
                    }

                    client.SetSubscriptions(subscriptions.ToArray());
                    await communicator.Start();
                    exitEvent.WaitOne(TimeSpan.FromSeconds(3));
                }
            }
        }
        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_url)
                .Build();

            await hubConnection.StartAsync();
        }

        public async Task SendAllRates(PriceUpdate priceUpdate)
        {
            if(hubConnection.State == HubConnectionState.Disconnected || hubConnection.State == HubConnectionState.Reconnecting)
            {
                await ConnectToHub();
            }
            await hubConnection.SendAsync("AllRates", priceUpdate);
        }

        public  Task GetSymbolsPrice() => Task.FromResult(_price);
        


    }
}
