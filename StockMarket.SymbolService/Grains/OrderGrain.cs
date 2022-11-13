using Newtonsoft.Json;
using Orleans.Concurrency;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.Observers;

namespace StockMarket.SymbolService.Grains
{
    [Reentrant]
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;
        private Order? _order;
        private IUserGrain? _userGrain;
        private double reservedUSDT = 0;
        INotifyObserver _observer;

        public override async Task OnActivateAsync()
        {
            _observer = new NotifyObserver();
        }
        private async Task ProcessOrder(bool stopped)
        {
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

                await _observer.Notify("NotifyOrder", "NotifyOrderProcess", new OrderInProcess
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

                    await _observer.Notify("NotifyOrder", "NotifyCloseOrderProcess", new OrderInProcess
                    {
                        Bid = _order.Bid,
                        Currency = _order.Currency,
                        Id = _order.Id,
                        NumberOf = _order.NumberOf,
                        User = _order.User,
                        CurrentAmmount = Convert.ToDouble(stockData?.Data.Amount)
                    });


                    var walltetStatusAfterOrder = await _userGrain.GetWallet();
                    await _observer.Notify("NotifyWallet", walltetStatusAfterOrder, _order.User);
                    break;

                }
            }
        }

        private async Task OrderStart()
        {
            reservedUSDT = _order.Bid * _order.NumberOf;
            await _userGrain.RemoveUsdt(reservedUSDT);
            var walltetStatus = await _userGrain.GetWallet();
            await _observer.Notify("NotifyWallet", walltetStatus, _order.User);
        }

        private async Task OrderStop()
        {
            _processStatus = false;
            await _userGrain.AddUSDT(reservedUSDT);
            reservedUSDT = 0;

            var walltetStatus = await _userGrain.GetWallet();
            await _observer.Notify("NotifyWallet", walltetStatus, _order.User);

            await _observer.Notify("NotifyOrder", "NotifyCloseOrderProcess", new OrderInProcess
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
