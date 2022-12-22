using Moq;
using Orleans.Runtime;
using StockMarket.Common.Models;
using StockMarket.SymbolService.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Tests
{
    public class UsersGrainTests
    {
        IPersistentState<List<User>> state;
        UsersGrain grain;
        public UsersGrainTests()
        {
            state = Mock.Of<IPersistentState<List<User>>>(_ => _.State == Mock.Of<List<User>>());
            grain = new UsersGrain(state);
        }
        [Fact]
        public async Task AddToUsers_State()
        {
            //Mock
            var user = Mock.Of<User>();

            //Act
            await grain.AddUser(user);

            //Assert
            Mock.Get(state).Verify(_ => _.WriteStateAsync());
            Assert.Equal(user, state.State[0]);
        }

        

        [Fact]
        public async Task GetUsers_State()
        {
            //Act
            var result = await grain.GetAllUsers();

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUser_State()
        {
            //Mock
            var user = Mock.Of<User>();

            //Act
            await grain.AddUser(user);
            var result = await grain.GetUser(user.Name);

            //Assert
            Mock.Get(state).Verify(_ => _.WriteStateAsync());
            Assert.Equal(user, state.State[0]);
            Assert.Equal(user, result);
        }
    }
}
