using Orleans;
using StockMarket.Common;
using SignalR.Orleans.Clients;
using Orleans.Hosting;

namespace StockMarket.API
{
    public class ClusterClientHostedService : IHostedService
    {
        public IClusterClient Client { get; }
        public ClusterClientHostedService(ILoggerProvider loggerProvider)
        {
            Client = new ClientBuilder()
                .UseLocalhostClustering(serviceId: "ChatSampleApp", clusterId: "dev")
                .UseSignalR()
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(IClientGrain).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(IStockSymbolsPriceGrain).Assembly).WithReferences();
                })
                .ConfigureLogging(builder => builder.AddProvider(loggerProvider))
                .Build();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Client.Connect(CreateRetryFilter());

            var grain = Client.GetGrain<IStockSymbolsPriceGrain>("all-rates");
            
            await Task.Factory.StartNew(async () => grain.GetSymbolsPrice().ConfigureAwait(false));
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
    }

}
