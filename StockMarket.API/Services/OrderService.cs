using Orleans;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IClusterClient _client;
        public OrderService(IClusterClient client)
        {
            _client = client;
        }
        public async Task CancelOrder(Order order)
        {
            var grain = _client.GetGrain<IOrderGrain>(order.Id);
            await Task.Factory.StartNew(() => Task.FromResult(grain.CancelOrder().ConfigureAwait(false)));
        }

        public async Task CreateOrder(Order order)
        {
            var grain = _client.GetGrain<IOrderGrain>(order.Id);

            await Task.Factory.StartNew(() => Task.FromResult(grain.CreateOrder(order).ConfigureAwait(false)));
        }
    }
}
