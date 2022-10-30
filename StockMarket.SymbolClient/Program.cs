using Orleans;
using Orleans.Hosting;
using Spectre.Console;
using StockMarket.Common;
using StockMarket.SymbolClient;
using System.Text;

var client = new ClientBuilder()
    .UseLocalhostClustering()
    .ConfigureApplicationParts(
        parts => parts.AddApplicationPart(typeof(IStockSymbolReaderGrain).Assembly).WithReferences())
    .AddSimpleMessageStreamProvider("chat")
    .Build();

ClientContext context = new(client);

await StartAsync(context);

var symbol1 = "{\"type\":\"subscribe\",\"symbol\":\"BINANCE:BTCUSDT\"}";
await SendMessage(context, "dangaUser1", symbol1);

var symbol2 = "{\"type\":\"subscribe\",\"symbol\":\"BINANCE:ETHUSDT\"}";
await SendMessage(context, "dangaUser2", symbol2);

static Task StartAsync(ClientContext context) =>
    AnsiConsole.Status().StartAsync("Connecting to server", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Dots);
        ctx.Status = "Connecting...";

        await context.Client.Connect(async error =>
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] error connecting to server!");
            AnsiConsole.WriteException(error);
            ctx.Status = "Waiting to retry...";
            await Task.Delay(TimeSpan.FromSeconds(2));
            ctx.Status = "Retrying connection...";
            return true;
        });

        ctx.Status = "Connected!";
    });

static async Task SendMessage(
    ClientContext context, string user,
    string symbol)
{

    var symbolReader = context.Client.GetGrain<IStockSymbolReaderGrain>(user);
    var symbolBites = Encoding.ASCII.GetBytes(symbol);
    await symbolReader.GetSymbolRate(symbolBites);
}