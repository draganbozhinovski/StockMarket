using Microsoft.AspNetCore.SignalR.Client;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.SymbolService.Grains
{
    public class GrainBase : Grain
    {
        public const string StockEndpoint = "https://api.coinbase.com/v2/prices/";
        public HubConnection hubConnection;
    }
}
