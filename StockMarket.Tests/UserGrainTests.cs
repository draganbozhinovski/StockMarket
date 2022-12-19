using Moq;
using Orleans;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.Grains;

namespace StockMarket.Tests
{
    public class UserGrainTests
    {
        IGrainFactory walletGrainFactory;
        IGrainFactory usersGrainFactory;
        IUsersGrain usersGrain;
        IWalletGrain walletGrain;
        IPersistentState<List<WalletCurrency>> walletState;
        IPersistentState<List<User>> usersState;

        Mock<UserGrain> grain;
        public UserGrainTests()
        {
            walletState = Mock.Of<IPersistentState<List<WalletCurrency>>>(_ => _.State == Mock.Of<List<WalletCurrency>>());
            usersState = Mock.Of<IPersistentState<List<User>>>(_ => _.State == Mock.Of<List<User>>());
            usersGrain = new UsersGrain(usersState);
            walletGrain = new WalletGrain(walletState);

            walletGrainFactory = Mock.Of<IGrainFactory>(_ => _.GetGrain<IWalletGrain>(Guid.Empty, null) == walletGrain);
            usersGrainFactory = Mock.Of<IGrainFactory>(_ => _.GetGrain<IUsersGrain>(0, null) == usersGrain);
            grain = new Mock<UserGrain>() { CallBase = true };

            grain.Setup(_ => _.WalletGrainFactory).Returns(walletGrainFactory);
            grain.Setup(_ => _.GrainKey).Returns("Wallet");
            grain.Setup(_ => _.UsersGrainFactory).Returns(usersGrainFactory);
            grain.Setup(_ => _.GrainKey).Returns("Users");
        }

        [Fact]
        public async Task CreatesUser()
        {
            //Mock
            var user = Mock.Of<User>();

            //Act
            await grain.Object.OnActivateAsync();
            var createdUser = await grain.Object.CreateUser(user);

            //Assert
            Assert.Equal(user, createdUser);
        }

        [Fact]
        public async Task AddsUSDT()
        {
            //Mock
            var ammount = 0;

            //Act
            await grain.Object.OnActivateAsync();
            var wallet = await grain.Object.AddUSDT(ammount);

            //Assert
            Assert.True(wallet.Any());
        }

        [Fact]
        public async Task RemovesUSDT()
        {
            //Mock
            var ammount = 0;

            //Act
            await grain.Object.OnActivateAsync();
            var wallet = await grain.Object.RemoveUsdt(ammount);

            //Assert
            Assert.False(wallet.Any());
        }

        [Fact]
        public async Task AddsToWallet()
        {
            //Mock
            var walletCurrency = Mock.Of<WalletCurrency>();

            //Act
            await grain.Object.OnActivateAsync();
            var wallet = await grain.Object.AddToWallet(walletCurrency);

            //Assert
            Assert.True(wallet.Any());
        }

        [Fact]
        public async Task RemovesFromWallet()
        {
            //Mock
            var walletCurrency = Mock.Of<WalletCurrency>();

            //Act
            await grain.Object.OnActivateAsync();
            var wallet = await grain.Object.RemoveFromWallet(walletCurrency);

            //Assert
            Assert.False(wallet.Any());
        }

        [Fact]
        public async Task GetsWallet()
        {
            //Mock
            var walletCurrency = Mock.Of<WalletCurrency>();

            //Act
            await grain.Object.OnActivateAsync();
            var walletOnAdd = await grain.Object.AddToWallet(walletCurrency);
            var wallet = await grain.Object.GetWallet();

            //Assert
            Assert.True(wallet.Any());
            Assert.Equal(walletOnAdd, wallet);
        }
    }
}
