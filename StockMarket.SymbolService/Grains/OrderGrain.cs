using Binance.Client.Websocket.Client;
using Binance.Client.Websocket.Subscriptions;
using Binance.Client.Websocket.Websockets;
using Binance.Client.Websocket;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using StockMarket.Common;
using System.Globalization;

namespace StockMarket.SymbolService.Grains
{
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private Order? _order;
        private IUserGrain? _userGrain;
        private Uri _url = new Uri("https://localhost:7015/notificationhub");
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public override async Task OnActivateAsync()
        {

            await ConnectToHub();

            await base.OnActivateAsync();
        }


        private async Task UpdateBinancePrice(Order order)
        {
            var exitEvent = new ManualResetEvent(false);

            var usdtToRemove = order.Bid * order.NumberOf;
            await _userGrain.RemoveUsdt(usdtToRemove);
            var walltetStatus = await _userGrain.GetWallet();
            await NotifyWallet(walltetStatus, order.User);

            var url = BinanceValues.ApiWebsocketUrl;
            using (var communicator = new BinanceWebsocketCommunicator(url))
            {
                using (var client = new BinanceWebsocketClient(communicator))
                {
                    client.Streams.TradesStream.Subscribe(async response =>
                    {
                        await semaphore.WaitAsync();
                        try
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

                            var stopProcess = await ProcessOrder(order, priceUpdate);
                            if (stopProcess)
                            {
                                exitEvent.WaitOne();
                            }
                            Console.WriteLine(JsonConvert.SerializeObject(priceUpdate));
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                    });

                    List<SubscriptionBase> subscriptions = new List<SubscriptionBase>();

                    subscriptions.Add(new TradeSubscription($"{order.Currency.ToString().ToLower()}usdt"));


                    client.SetSubscriptions(subscriptions.ToArray());
                    await communicator.Start();
                    exitEvent.WaitOne(TimeSpan.FromDays(3));
                }
            }
        }

        private async Task<bool> ProcessOrder(Order order, PriceUpdate price)
        {

            await NotifyOrder("NotifyOrderProcess", new OrderInProcess
            {
                Bid = order.Bid,
                Currency = order.Currency,
                Id = order.Id,
                NumberOf = order.NumberOf,
                User = order.User,
                CurrentAmmount = price.Data.Amount
            });

            if (price.Data.Amount <= order.Bid)
            {
                //minus for processing the order to the platform account with observable                    
                var currencyToAdd = new WalletCurrency
                {
                    Ammount = order.NumberOf,
                    Currency = order.Currency
                };
                await _userGrain.AddToWallet(currencyToAdd);

                //notify all users for the orpdr processed with observable

                await NotifyOrder("NotifyCloseOrderProcess", new OrderInProcess
                {
                    Bid = order.Bid,
                    Currency = order.Currency,
                    Id = order.Id,
                    NumberOf = order.NumberOf,
                    User = order.User,
                    CurrentAmmount = price.Data.Amount
                });


                var walltetStatusAfterOrder = await _userGrain.GetWallet();
                await NotifyWallet(walltetStatusAfterOrder, order.User);
                return true;
            }
            return false;

        }



        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_url)
                .Build();

            await hubConnection.StartAsync();
        }

        public async Task NotifyOrder(string method, OrderInProcess orderInProcess)
        {
            if (hubConnection.State == HubConnectionState.Disconnected || hubConnection.State == HubConnectionState.Reconnecting)
            {
                await ConnectToHub();
            }
            await hubConnection.SendAsync("NotifyOrder", method, orderInProcess);
        }

        public async Task NotifyWallet(List<WalletCurrency> walletCurrencies, User user)
        {
            if (hubConnection.State == HubConnectionState.Disconnected || hubConnection.State == HubConnectionState.Reconnecting)
            {
                await ConnectToHub();
            }
            await hubConnection.SendAsync("NotifyWallet", walletCurrencies, user);
        }


        public Task CreateOrder(Order order)
        {
            _order = order;
            _userGrain = GrainFactory.GetGrain<IUserGrain>(_order.User.Id);
            return UpdateBinancePrice(order); ;
        }


    }
}
