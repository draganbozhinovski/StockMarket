using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;





await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
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