using Orleans;
using StockMarket.Common;

namespace StockMarket.API.Services
{
    public class UserService : IUserService
    {
        private readonly IClusterClient _client;
        public UserService(IClusterClient client)
        {
            _client = client;
        }

        public async Task<List<WalletCurrency>> AddUSDT(Usdt usdt)
        {
            var grain = _client.GetGrain<IUserGrain>(usdt.UserId);
            return await grain.AddUSDT(usdt.Ammount);
        }

        public async Task<User> CreateUser(User user)
        {
            var grain = _client.GetGrain<IUserGrain>(user.Id);
            return await grain.CreateUser(user.Name);
        }

        public async Task<List<WalletCurrency>> GetWallet(Guid userId)
        {
            var grain = _client.GetGrain<IUserGrain>(userId);
            return await grain.GetWallet();
        }


    }
}
