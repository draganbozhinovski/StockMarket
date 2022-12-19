using Moq;
using Orleans;
using StockMarket.Common;
using StockMarket.Common.Models;
using StockMarket.SymbolService.Grains;
using StockMarket.SymbolService.HubClient;

namespace StockMarket.Tests
{
    public class OrderGrainTests
    {
        INotifier notifier;
        IUserOrdersGrain userOrdersGrain;
        IUserGrain userGrain;
        IGrainFactory userOrdersFactory;
        IGrainFactory userFactory;
        Mock<OrderGrain> grain;
        public OrderGrainTests()
        {
            notifier = Mock.Of<INotifier>();
            userOrdersGrain = Mock.Of<IUserOrdersGrain>();
            userGrain = Mock.Of<IUserGrain>();
            userOrdersFactory = Mock.Of<IGrainFactory>(_ => _.GetGrain<IUserOrdersGrain>(Guid.Empty, null) == userOrdersGrain);
            userFactory = Mock.Of<IGrainFactory>(_ => _.GetGrain<IUserGrain>(Guid.Empty, null) == userGrain);
            grain = new Mock<OrderGrain>(notifier) { CallBase = true };

            grain.Setup(_ => _.UserOrdersGrainFactory).Returns(userOrdersFactory);
            grain.Setup(_ => _.GrainKey).Returns("UserOrders");
            grain.Setup(_ => _.UserGrainFactory).Returns(userFactory);
            grain.Setup(_ => _.GrainKey).Returns("User");            
        }

        [Fact]
        public async Task CreatesOrder_From_User()
        {
            //Mock
            var order = Mock.Of<Order>();
            order.User = Mock.Of<User>();

            //Act
            grain.Object._processStatus = false;
            await grain.Object.CreateOrder(order, false);

            //Assert
            Mock.Get(userOrdersGrain).Verify(_ => _.AddUserOrder(order));
            Mock.Get(userGrain).Verify(_ => _.RemoveUsdt(order.Bid * order.NumberOf));
            Mock.Get(userGrain).Verify(_ => _.GetWallet());

        }

        [Fact]
        public async Task CreatesOrder_From_Rehydrate()
        {
            //Mock
            var order = Mock.Of<Order>();
            order.User = Mock.Of<User>();

            //Act
            grain.Object._processStatus = false;
            await grain.Object.CreateOrder(order, true);

            //Assert
            Mock.Get(userOrdersGrain).Verify(_ => _.AddUserOrder(order));
        }

        [Fact]
        public async Task CancelOrder()
        {
            //Mock
            var order = Mock.Of<Order>();
            order.User = Mock.Of<User>();

            //Act
            grain.Object._processStatus = false;
            await grain.Object.CreateOrder(order, true);
            await grain.Object.CancelOrder();

            //Assert;
            Mock.Get(userOrdersGrain).Verify(_ => _.RemoveUserOrder(order));
            Mock.Get(userGrain).Verify(_ => _.AddUSDT(order.Bid * order.NumberOf));
            Mock.Get(userGrain).Verify(_ => _.GetWallet());;

        }
    }
}
