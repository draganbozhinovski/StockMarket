using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task CreateUser(string name);
        Task AddUSDT(double ammount);
        Task AddToWallet(WalletCurrency walletCurrency);
        Task RemoveFromWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> GetWallet();

    }
}
