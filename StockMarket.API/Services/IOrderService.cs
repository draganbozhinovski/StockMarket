using StockMarket.Common.Models;

namespace StockMarket.API.Services
{
    public interface IOrderService
    {
        Task CreateOrder(Order order);
        Task CancelOrder(Order order);
    }
}
