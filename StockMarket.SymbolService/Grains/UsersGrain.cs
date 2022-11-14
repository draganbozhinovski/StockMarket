using Orleans;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.Grains
{
    public class UsersGrain : Grain, IUsersGrain
    {
        private readonly IPersistentState<User> _users;
        public UsersGrain(
            [PersistentState("userProfile", "profileStore")]
            IPersistentState<User> users
            )
        {
            _users = users;
        }
        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }
        public async Task AddUser(User user)
        {
            _users.State = user;
            await _users.WriteStateAsync();
        }

        public async Task<User?> GetUser(string name)
        {
             await _users.ReadStateAsync();
            return await Task.FromResult(user);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await Task.FromResult(_users);
        }
    }
}
