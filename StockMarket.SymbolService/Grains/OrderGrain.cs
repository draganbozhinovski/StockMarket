using Newtonsoft.Json;
using Orleans.Concurrency;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.HubClient;

namespace StockMarket.SymbolService.Grains
{
    [Reentrant]
    public class OrderGrain : GrainBase, IOrderGrain
    {
        private bool _processStatus = true;
        private Order? _order;
        private IUserGrain? _userGrain;
        private IUserOrdersGrain? _userOrdersGrain;
        private double reservedUSDT = 0;
        INotifier _notifier;

        public override async Task OnActivateAsync()
        {
            _notifier = new Notifier();
        }

        public async Task CreateOrder(Order order, bool isRehydrate)
        {
            _order = order;
            _userGrain = GrainFactory.GetGrain<IUserGrain>(_order.User.Id);
            _userOrdersGrain = GrainFactory.GetGrain<IUserOrdersGrain>(_order.User.Id);
            await _userOrdersGrain.AddUserOrder(order);
            await ProcessOrder(stopped: false, isRehydrate);
        }

        public async Task CancelOrder()
        {
            _processStatus = false;
            await _userOrdersGrain.RemoveUserOrder(_order);
            await ProcessOrder(stopped: true, false);
        }

        private async Task ProcessOrder(bool stopped, bool isRehydrate)
        {
            if (!stopped && !isRehydrate)
            {
                await OrderStart();
            }
            if (stopped)
            {
                await OrderStop();
                return;
            }
            while (_processStatus)
            {
                var priceData = await GetPriceQuote(_order.Currency.ToString());
                if (!_processStatus)
                {
                    return;
                }
                await _notifier.Notify("NotifyOrder", "NotifyOrderProcess", new OrderInProcess
                {
                    Bid = _order.Bid,
                    Currency = _order.Currency,
                    Id = _order.Id,
                    NumberOf = _order.NumberOf,
                    User = _order.User,
                    CurrentAmmount = Convert.ToDouble(priceData?.Data.Amount)
                });

                if (Convert.ToDouble(priceData?.Data.Amount) <= _order.Bid)
                {
                    _processStatus = false;
                    await OrderSuccess(priceData);
                    
                    break;
                }
            }
        }

        private async Task OrderStart()
        {
            reservedUSDT = _order.Bid * _order.NumberOf;
            await _userGrain.RemoveUsdt(reservedUSDT);
            var walltetStatus = await _userGrain.GetWallet();
            await _notifier.Notify("NotifyWallet", walltetStatus, _order.User);
        }

        private async Task OrderStop()
        {
            _processStatus = false;
            await _userGrain.AddUSDT(reservedUSDT);
            reservedUSDT = 0;

            var walltetStatus = await _userGrain.GetWallet();
            await _notifier.Notify("NotifyWallet", walltetStatus, _order.User);

            await _notifier.Notify("NotifyOrder", "NotifyCloseOrderProcess", new OrderInProcess
            {
                Bid = _order.Bid,
                Currency = _order.Currency,
                Id = _order.Id,
                NumberOf = _order.NumberOf,
                User = _order.User,
                CurrentAmmount = 0
            });
        }

        private async Task OrderSuccess(PriceUpdate stockData)
        {
            var currencyToAdd = new WalletCurrency
            {
                Ammount = _order.NumberOf,
                Currency = _order.Currency
            };
            await _userGrain.AddToWallet(currencyToAdd);

            await _notifier.Notify("NotifyOrder", "NotifyCloseOrderProcess", new OrderInProcess
            {
                Bid = _order.Bid,
                Currency = _order.Currency,
                Id = _order.Id,
                NumberOf = _order.NumberOf,
                User = _order.User,
                CurrentAmmount = Convert.ToDouble(stockData.Data.Amount)
            });


            var walltetStatusAfterOrder = await _userGrain.GetWallet();
            await _notifier.Notify("NotifyWallet", walltetStatusAfterOrder, _order.User);
            await _userOrdersGrain.RemoveUserOrder(_order);
        }

        private async Task<PriceUpdate> GetPriceQuote(string currency)
        {
            using var resp =
                await _httpClient.GetAsync(
                    $"{StockEndpoint}{currency}-USD/buy");

            var priceData = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PriceUpdate>(priceData);
        }



    }
}
