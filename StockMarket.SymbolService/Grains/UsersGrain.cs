using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.Grains
{
    [StatelessWorker(1)]
    public class UsersGrain : Grain, IUsersGrain
    {
        private readonly IPersistentState<List<User>> _users;
        public UsersGrain(
            [PersistentState("userProfile", "profileStore")]
            IPersistentState<List<User>> users
            )
        {
            _users = users;
        }

        public async Task AddUser(User user)
        {
            if (!_users.State.Any(u => u.Name == user.Name))
            {
                _users.State.Add(user);
                await _users.WriteStateAsync();
            }
        }

        public async Task<User?> GetUser(string name)
        {
            var user = _users.State.FirstOrDefault(u => u.Name == name);
            return await Task.FromResult(user);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await Task.FromResult(_users.State);
        }
    }
}
