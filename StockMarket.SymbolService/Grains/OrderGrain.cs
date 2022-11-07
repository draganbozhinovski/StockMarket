using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Orleans;
using StockMarket.Common;
using System.Globalization;

namespace StockMarket.SymbolService.Grains
{
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;
        private Order? _order;
        private IUserGrain? _userGrain;
        private Uri _url = new Uri("https://localhost:7015/notificationhub");

        public override async Task OnActivateAsync()
        {
            await ConnectToHub();
        }
        private async Task ProcessOrder(Order order)
        {
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                await ConnectToHub();
            }

            var usdtToRemove = order.Bid * order.NumberOf;
            await _userGrain.RemoveUsdt(usdtToRemove);
            var walltetStatus = await _userGrain.GetWallet();
            await NotifyWallet(walltetStatus, order.User);

            while (_processStatus)
            {
                var price = await GetPriceQuote(order.Currency.ToString());
                var stockData = JsonConvert.DeserializeObject<PriceUpdate>(price);

                await NotifyOrder("NotifyOrderProcess", new OrderInProcess
                {
                    Bid = order.Bid,
                    Currency = order.Currency,
                    Id = order.Id,
                    NumberOf = order.NumberOf,
                    User = order.User,
                    CurrentAmmount = Convert.ToDouble(stockData?.Data.Amount)
                });               

                if (Convert.ToDouble(stockData?.Data.Amount) <= order.Bid)
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
                        CurrentAmmount = Convert.ToDouble(stockData?.Data.Amount)
                    });


                    var walltetStatusAfterOrder = await _userGrain.GetWallet();
                    await NotifyWallet(walltetStatusAfterOrder, order.User);

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


        public Task CreateOrder(Order order)
        {
            _order = order;
            _userGrain = GrainFactory.GetGrain<IUserGrain>(_order.User.Id);
            return ProcessOrder(order); ;
        }


    }
}
