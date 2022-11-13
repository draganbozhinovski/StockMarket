using Orleans;
using StockMarket.Common.Models;

namespace StockMarket.Common
{
    public interface IOrderGrain : IGrainWithGuidKey
    {
        Task CreateOrder(Order order);
        Task CancelOrder();
    }
}
