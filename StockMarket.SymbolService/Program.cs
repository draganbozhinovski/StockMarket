using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using StockMarket.Common;
using StockMarket.SymbolService;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseSignalR(builder =>
            {
                builder
                    .Configure((innerSiloBuilder, config) =>
                    {
                        innerSiloBuilder
                            .UseLocalhostClustering(serviceId: "ChatSampleApp", clusterId: "dev")
                            .AddMemoryGrainStorage("PubSubStore")
                            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(StockSymbolsPriceGrain).Assembly).WithReferences());
                    });
            })
            .UseLocalhostClustering()
            .UseDashboard(x =>
            {
                x.HostSelf = true;
                x.Port = 4387;
                x.Host = "*";
                x.CounterUpdateIntervalMs = 1000;
            })
            .AddMemoryGrainStorage("PubSubStore")
            .AddSimpleMessageStreamProvider("chat", options =>
            {
                options.FireAndForgetDelivery = true;
            });

    })
    .RunConsoleAsync();