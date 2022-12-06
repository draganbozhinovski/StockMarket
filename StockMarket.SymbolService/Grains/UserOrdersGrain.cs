using Orleans;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.SymbolService.Grains
{
    public class UserOrdersGrain : Grain, IUserOrdersGrain
    {
        private readonly IPersistentState<List<Order>> _orders;
        public UserOrdersGrain(
            [PersistentState("userOrders", "profileStore")]
            IPersistentState<List<Order>> orders)
        {
            _orders = orders;
        }

        public async Task AddUserOrder(Order order)
        {
            _orders.State.Add(order);
            await _orders.WriteStateAsync();
        }
        public async Task RemoveUserOrder(Order order)
        {
            var stateOrder = _orders.State.FirstOrDefault(x=>x.Id == order.Id);
            _orders.State.Remove(stateOrder);
            await _orders.WriteStateAsync();
        }
        public async Task<IEnumerable<Order>> GetUserOrders()
        {
            return await Task.FromResult(_orders.State);
        }
    }
}
