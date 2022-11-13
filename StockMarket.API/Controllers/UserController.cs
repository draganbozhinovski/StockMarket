using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockMarket.API.Services;
using StockMarket.Common.Models;

namespace StockMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<User> CreateUser(User user)
        {
            return await _userService.CreateUser(user);
        }

        [HttpPost]
        [Route("AddUSDT")]
        public async Task<List<WalletCurrency>> AddUSDT(Usdt usdt)
        {
            return await _userService.AddUSDT(usdt);
        }
        [HttpGet]
        [Route("Wallet/{userId}")]
        public async Task<List<WalletCurrency>> GetWallet(Guid userId)
        {
            return await _userService.GetWallet(userId);
        }
    }
}
