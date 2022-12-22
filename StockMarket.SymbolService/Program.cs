using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using StockMarket.SymbolService.HubClient;
using System.Net;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .ConfigureEndpoints(IPAddress.Parse("127.0.0.1"), 11111, 30000)
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "StockSymbol";
                options.ServiceId = "StockSymbolService";
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<INotifier, Notifier>();
            })
            .UseAzureStorageClustering(options =>
            {
                options.ConfigureTableServiceClient("DefaultEndpointsProtocol=https;AccountName=stocksymbolservice;AccountKey=***********************************;" +
                                                    "BlobEndpoint=https://**************.blob.core.windows.net/;" +
                                                    "QueueEndpoint=https://**************.queue.core.windows.net/;" +
                                                    "TableEndpoint=https://**************.table.core.windows.net/;" +
                                                    "FileEndpoint=https://**************.file.core.windows.net/;");
            })
            .AddAzureBlobGrainStorage(
                    name: "profileStore",
                    configureOptions: options =>
                    {
                        options.UseJson = true;
                        options.ConfigureBlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stocksymbolservice;AccountKey=***********************************");
                    }
            )
            .UseDashboard(x =>
            {
                x.HostSelf = true;
                x.Port = 4387;
                x.Host = "*";
                x.CounterUpdateIntervalMs = 1000;
            });

    })
    .RunConsoleAsync();