using Orleans;

namespace StockMarket.SymbolService.Observers
{
    public interface IOrderObserver : IGrainObserver
    {
        Task NotifyAllUsers(string method, object message);
    }
}
