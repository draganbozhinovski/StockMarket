using Orleans;
using StockMarket.Common;
using StockMarket.Common.Models;

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
            var existingUsersGrain = _client.GetGrain<IUsersGrain>(0);
            var existingUser = await existingUsersGrain.GetUser(user.Name);
            if (existingUser != null)
            {
                return existingUser;
            }
            else
            {
                user.Id = Guid.NewGuid();
                var userGrain = _client.GetGrain<IUserGrain>(user.Id);
                return await userGrain.CreateUser(user.Name);
            }
        }

        public async Task<List<WalletCurrency>> GetWallet(Guid userId)
        {
            var grain = _client.GetGrain<IUserGrain>(userId);
            return await grain.GetWallet();
        }




    }
}
