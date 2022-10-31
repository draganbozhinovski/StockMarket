using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using StockMarket.Common;
using System.Text;

namespace StockMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IClusterClient _client;
        public OrderController(IClusterClient client)
        {
            _client = client;
        }


        [HttpPost]
        [Route("createorder")]
        public async Task Create([FromBody] Order order)
        {
            order.Id = Guid.NewGuid();

            var grain = _client.GetGrain<IOrderGrain>(order.Id);

            await Task.Factory.StartNew(() => Task.FromResult(grain.CreateOrder(order).ConfigureAwait(false)));
        }

        [HttpPost]
        [Route("createUser")]
        public async Task CreateUser([FromBody] User user)
        {
            var grain = _client.GetGrain<IUserGrain>(user.Id);

            await Task.Factory.StartNew(() => Task.FromResult(grain.CreateUser(user.Name).ConfigureAwait(false)));
        }

        [HttpPost]
        [Route("AddUSDT/{ammount}/{userId}")]
        public async Task AddUSDT([FromRoute] double ammount, [FromRoute] Guid userId)
        {
            var grain = _client.GetGrain<IUserGrain>(userId);

            await Task.Factory.StartNew(() => Task.FromResult(grain.AddUSDT(ammount).ConfigureAwait(false)));
        }

        [HttpPost]
        [Route("GetWallet/{userId}")]
        public async Task<List<WalletCurrency>> GetWallet( [FromRoute] Guid userId)
        {
            var grain = _client.GetGrain<IUserGrain>(userId);

            return await grain.GetWallet();
        }


    }
}
