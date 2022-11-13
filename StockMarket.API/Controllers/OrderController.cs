using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using StockMarket.API.Services;
using StockMarket.Common.Models;
using System.Text;

namespace StockMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }


        [HttpPost]
        [Route("create")]
        public async Task Create([FromBody] Order order)
        {
            await _orderService.CreateOrder(order);
        }

        [HttpPost]
        [Route("cancel")]
        public async Task Cancel([FromBody] Order order)
        {
            await _orderService.CancelOrder(order);
        }




    }
}
