using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using StockMarket.Common;
using System.Globalization;

namespace StockMarket.SymbolService.Grains
{
    [Reentrant]
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;
        private Order? _order;
        private IUserGrain? _userGrain;
        private double reservedUSDT = 0;

        public override async Task OnActivateAsync()
        {
            await ConnectToHub();
        }
        private async Task ProcessOrder(bool stopped)
        {
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                await ConnectToHub();
            }

            if (!stopped)
            {
                await OrderStart();
            }
            else
            {
                await OrderStop();
                return;
            }

            while (_processStatus)
            {
                var price = await GetPriceQuote(_order.Currency.ToString());
                var stockData = JsonConvert.DeserializeObject<PriceUpdate>(price);

                await NotifyOrder("NotifyOrderProcess", new OrderInProcess
                {
                    Bid = _order.Bid,
                    Currency = _order.Currency,
                    Id = _order.Id,
                    NumberOf = _order.NumberOf,
                    User = _order.User,
                    CurrentAmmount = Convert.ToDouble(stockData?.Data.Amount)
                });

                if (Convert.ToDouble(stockData?.Data.Amount) <= _order.Bid)
                {
                    _processStatus = false;
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
                        CurrentAmmount = Convert.ToDouble(stockData?.Data.Amount)
                    });


                    var walltetStatusAfterOrder = await _userGrain.GetWallet();
                    await NotifyWallet(walltetStatusAfterOrder, _order.User);
                    break;

                }
            }
        }

        private async Task OrderStart()
        {
            reservedUSDT = _order.Bid * _order.NumberOf;
            await _userGrain.RemoveUsdt(reservedUSDT);
            var walltetStatus = await _userGrain.GetWallet();
            await NotifyWallet(walltetStatus, _order.User);
        }

        private async Task OrderStop()
        {
            _processStatus = false;
            await _userGrain.AddUSDT(reservedUSDT);
            reservedUSDT = 0;
            var walltetStatus = await _userGrain.GetWallet();
            await NotifyWallet(walltetStatus, _order.User);
            await NotifyOrder("NotifyCloseOrderProcess", new OrderInProcess
            {
                Bid = _order.Bid,
                Currency = _order.Currency,
                Id = _order.Id,
                NumberOf = _order.NumberOf,
                User = _order.User,
                CurrentAmmount = 0
            });
        }

        private async Task<string> GetPriceQuote(string currency)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{currency}-USD/buy");

            return await resp.Content.ReadAsStringAsync();
        }

        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
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
            await ProcessOrder(stopped: false); ;
        }

        public async Task CancelOrder()
        {
            await ProcessOrder(stopped: true);

        }


    }
}
