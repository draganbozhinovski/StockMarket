using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.SymbolClient
{
    internal readonly record struct ClientContext(IClusterClient Client) { }
}
