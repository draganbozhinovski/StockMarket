using Moq;
using StockMarket.SymbolService.Grains;
using StockMarket.SymbolService.HubClient;

namespace StockMarket.Tests
{
    public class CurrenciesPriceGrainTests
    {
        [Fact]
        public async Task TimerWillTrigger()
        {
            // mock the grain to override methods
            var notifier = new Mock<INotifier>() { CallBase = true };
            var grain = new Mock<CurrenciesPriceGrain>(notifier.Object) { CallBase = true };

            // mock the timer registration method and capture the action
            Func<object, Task> action = null;
            object state = null;
            var dueTime = TimeSpan.FromSeconds(1);
            var period = TimeSpan.FromSeconds(1);

            grain.Setup(_ => _.RegisterTimer(It.IsAny<Func<object, Task>>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) => { action = a; state = b; dueTime = c; period = d; })
                .Returns(Mock.Of<IDisposable>());

            // simulate activation
            await grain.Object.OnActivateAsync();
            await grain.Object.StartSymbolsPrice();

            // assert the timer was registered
            Assert.NotNull(action);
            Assert.Null(state);
            Assert.Equal(TimeSpan.FromSeconds(1), dueTime);
            Assert.Equal(TimeSpan.FromSeconds(1), period);


            // tick the timer
            await action(null);

        }
    }
}