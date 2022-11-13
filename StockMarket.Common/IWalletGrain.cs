using Orleans;
using StockMarket.Common.Models;

namespace StockMarket.Common
{
    public interface IWalletGrain : IGrainWithGuidKey
    {
        Task<List<WalletCurrency>> AddToWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> RemoveFromWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> GetWallet();
    }
}
