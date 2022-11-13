using Orleans;
using StockMarket.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public interface IUsersGrain : IGrainWithIntegerKey
    {
        Task AddUser(User user);
        Task<User> GetUser(string name);
        Task<IEnumerable<User>> GetAllUsers();
    }
}
