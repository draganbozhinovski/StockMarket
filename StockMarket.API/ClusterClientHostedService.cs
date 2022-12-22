using Orleans;
using StockMarket.Common;
using Orleans.Hosting;

namespace StockMarket.API
{
    public class ClusterClientHostedService : IHostedService
    {
        public IClusterClient Client { get; }
        public ClusterClientHostedService(ILoggerProvider loggerProvider)
        {
            Client = new ClientBuilder()
                .UseLocalhostClustering()
                .ConfigureLogging(builder => builder.AddProvider(loggerProvider))
                .Build();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Client.Connect(CreateRetryFilter());

            var grain = Client.GetGrain<ICurrenciesPriceGrain>("all-rates");

            await Task.Factory.StartNew(() => Task.FromResult(grain.StartSymbolsPrice().ConfigureAwait(false)));
            await RehydrateOrders();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.Close();

            Client.Dispose();
        }

        private static Func<Exception, Task<bool>> CreateRetryFilter(int maxAttempts = 5)
        {
            var attempt = 0;
            return RetryFilter;

            async Task<bool> RetryFilter(Exception exception)
            {
                attempt++;
                Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
                if (attempt > maxAttempts)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            }
        }

        private async Task RehydrateOrders()
        {
            var usersGrain = Client.GetGrain<IUsersGrain>(0);
            var users = await usersGrain.GetAllUsers();
            foreach (var user in users)
            {
                var userOrdersGrain = Client.GetGrain<IUserOrdersGrain>(user.Id);
                var existingUserOrders = await userOrdersGrain.GetUserOrders();
                foreach (var order in existingUserOrders)
                {
                    var orderGrain = Client.GetGrain<IOrderGrain>(order.Id);
                    await Task.Factory.StartNew(() => Task.FromResult(orderGrain.CreateOrder(order, true).ConfigureAwait(false)));
                }
            }
        }
    }

}
