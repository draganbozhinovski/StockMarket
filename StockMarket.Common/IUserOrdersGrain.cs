using Orleans;
using StockMarket.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public interface IUserOrdersGrain : IGrainWithGuidKey
    {
        public Task AddUserOrder(Order order);
        public Task RemoveUserOrder(Order order);
        public Task<IEnumerable<Order>> GetUserOrders(); 
    }
}
