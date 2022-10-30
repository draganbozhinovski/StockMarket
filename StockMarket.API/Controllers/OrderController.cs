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
        public async Task Create([FromBody]Order order)
        {
            order.Id = Guid.NewGuid();

            var grain = _client.GetGrain<IOrderGrain>(order.Id);

            await Task.Factory.StartNew(() => Task.FromResult(grain.CreateOrder(order).ConfigureAwait(false)));
        }
    }
}
