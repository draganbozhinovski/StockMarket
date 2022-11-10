﻿using Binance.Client.Websocket.Client;
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
        IDisposable _timer;

        public override async Task OnActivateAsync()
        {

            await ConnectToHub();
            _timer = RegisterTimer(UpdateBinancePrice,
                                        this,
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(1));

            await base.OnActivateAsync();
        }


        private async Task UpdateBinancePrice(Object _)
        {
            var exitEvent = new ManualResetEvent(false);

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

                            var stopProcess = await ProcessOrder(priceUpdate);
                            if (stopProcess)
                            {
                                _timer.Dispose();
                                exitEvent.WaitOne(TimeSpan.FromMilliseconds(1));
                            }
                            Console.WriteLine(JsonConvert.SerializeObject(priceUpdate));
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                    });

                    List<SubscriptionBase> subscriptions = new List<SubscriptionBase>();

                    subscriptions.Add(new TradeSubscription($"{_order.Currency.ToString().ToLower()}usdt"));


                    client.SetSubscriptions(subscriptions.ToArray());
                    await communicator.Start();
                    exitEvent.WaitOne(TimeSpan.FromMilliseconds(2));
                }
            }
        }

        private async Task<bool> ProcessOrder(PriceUpdate price)
        {

            await NotifyOrder("NotifyOrderProcess", new OrderInProcess
            {
                Bid = _order.Bid,
                Currency = _order.Currency,
                Id = _order.Id,
                NumberOf = _order.NumberOf,
                User = _order.User,
                CurrentAmmount = price.Data.Amount
            });

            if (price.Data.Amount <= _order.Bid)
            {
                //minus for processing the _order to the platform account with observable                    
                var currencyToAdd = new WalletCurrency
                {
                    Ammount = _order.NumberOf,
                    Currency = _order.Currency
                };
                await _userGrain.AddToWallet(currencyToAdd);

                //notify all users for the orpdr processed with observable

                await NotifyOrder("NotifyCloseOrderProcess", new OrderInProcess
                {
                    Bid = _order.Bid,
                    Currency = _order.Currency,
                    Id = _order.Id,
                    NumberOf = _order.NumberOf,
                    User = _order.User,
                    CurrentAmmount = price.Data.Amount
                });


                var walltetStatusAfterOrder = await _userGrain.GetWallet();
                await NotifyWallet(walltetStatusAfterOrder, _order.User);
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
            await hubConnection.SendAsync("NotifyOrder", method, orderInProcess);
        }

        public async Task NotifyWallet(List<WalletCurrency> walletCurrencies, User user)
        {
            await hubConnection.SendAsync("NotifyWallet", walletCurrencies, user);
        }


        public async Task CreateOrder(Order order)
        {
            _order = order;
            _userGrain = GrainFactory.GetGrain<IUserGrain>(_order.User.Id);
            var usdtToRemove = _order.Bid * _order.NumberOf;
            await _userGrain.RemoveUsdt(usdtToRemove);
            var walltetStatus = await _userGrain.GetWallet();
            await NotifyWallet(walltetStatus, _order.User);
            await Task.Delay(1);
        }


    }
}
