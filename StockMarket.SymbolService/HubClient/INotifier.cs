using Orleans;

namespace StockMarket.SymbolService.HubClient
{
    public interface INotifier
    {
        Task Notify(string method, object message);
        Task Notify(string method, string clientMethod, object message);
        Task Notify(string method, object message1, object message2);
    }
}
