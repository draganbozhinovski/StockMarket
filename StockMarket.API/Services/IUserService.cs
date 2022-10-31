using StockMarket.Common;

namespace StockMarket.API.Services
{
    public interface IUserService
    {
        Task<User> CreateUser(User user);
        Task<List<WalletCurrency>> AddUSDT(Usdt usdt);
        Task<List<WalletCurrency>> GetWallet(Guid userId);
    }
}
