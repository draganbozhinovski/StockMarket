using Moq;
using Orleans.Runtime;
using StockMarket.Common.Models;
using StockMarket.SymbolService.Grains;

namespace StockMarket.Tests
{
    public class WalletGrainTests
    {
        IPersistentState<List<WalletCurrency>> state;
        WalletGrain grain;
        public WalletGrainTests()
        {
            state = Mock.Of<IPersistentState<List<WalletCurrency>>>(_ => _.State == Mock.Of<List<WalletCurrency>>());
            grain = new WalletGrain(state);

        }
        [Fact]
        public async Task AddsToWallet_State()
        {
            //Mock
            var walletCurrency = new WalletCurrency
            {
                Ammount = 100,
                Currency = Currency.USDT
            };

            //Act            
            await grain.AddToWallet(walletCurrency);

            //Assert
            Mock.Get(state).Verify(_ => _.WriteStateAsync());
            Assert.Equal(walletCurrency, state.State[0]);
        }

        [Fact]
        public async Task RemovesFromWallet_State()
        {
            //Mock
            var walletCurrency = new WalletCurrency
            {
                Ammount = 100,
                Currency = Currency.USDT
            };

            //Act            
            await grain.RemoveFromWallet(walletCurrency);

            //Assert
            Mock.Get(state).Verify(_ => _.WriteStateAsync());
            Assert.True(!state.State.Any());
        }

        [Fact]
        public async Task GetsWallet_State()
        {
            //Act            
            var result = await grain.GetWallet();

            //Assert
            Assert.NotNull(result);
        }
    }
}
