using Orleans;

namespace StockMarket.Common
{
    public interface IStockSymbolGrain : IGrainWithStringKey
    {
        Task<string> GetPrice();
    }
}
