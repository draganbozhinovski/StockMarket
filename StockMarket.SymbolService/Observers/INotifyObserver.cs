using Orleans;

namespace StockMarket.SymbolService.Observers
{
    public interface INotifyObserver : IGrainObserver
    {
        Task Notify(string method, object message);
        Task Notify(string method, string clientMethod, object message);
        Task Notify(string method, object message1, object message2);
    }
}
