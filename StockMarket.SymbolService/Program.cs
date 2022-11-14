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
            .AddAzureTableGrainStorage(
                    name: "profileStore", 
                    configureOptions: options =>
                    {
                        options.UseJson = true;
                        options.ConfigureTableServiceClient("DefaultEndpointsProtocol=https;AccountName=stocksymbolservice;AccountKey=aM+XKkRgwu0ACb2kF8511TdmA2+B5IQxen5CIZVEEOq5Om0Lj1pTOmRXvnPt6e6p/vMMwHGoliAN+ASttOoYNg==");
                    }
            )
            //.AddMemoryGrainStorage("PubSubStore")
            .AddSimpleMessageStreamProvider("chat", options =>
            {
                options.FireAndForgetDelivery = true;
            });

    })
    .RunConsoleAsync();