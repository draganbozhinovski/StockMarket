using Orleans;

namespace StockMarket.Common
{
    public interface IWalletGrain : IGrainWithGuidKey
    {
        Task AddToWallet(WalletCurrency walletCurrency);
        Task RemoveFromWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> GetWallet();
    }
}
