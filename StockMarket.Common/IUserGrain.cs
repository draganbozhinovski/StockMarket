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
        Task<User> CreateUser(string name);
        Task<List<WalletCurrency>> AddUSDT(double ammount);
        Task<List<WalletCurrency>> RemoveUsdt(double ammount);
        Task<List<WalletCurrency>> AddToWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> RemoveFromWallet(WalletCurrency walletCurrency);
        Task<List<WalletCurrency>> GetWallet();

    }
}
