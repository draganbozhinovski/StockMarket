using Orleans;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    public class UsersGrain : Grain, IUsersGrain
    {
        private List<User> _users;

        public override Task OnActivateAsync()
        {
            _users = new List<User>();
            return base.OnActivateAsync();
        }
        public async Task AddUser(User user)
        {
            _users.Add(user);
            await Task.FromResult(user);
        }

        public async Task<User?> GetUser(string name)
        {
            var user = _users.FirstOrDefault(u => u.Name == name);
            return await Task.FromResult(user);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await new Task<IEnumerable<User>>(() => _users);
        }
    }
}
