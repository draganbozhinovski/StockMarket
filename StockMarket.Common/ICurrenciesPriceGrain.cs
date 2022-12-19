using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public interface ICurrenciesPriceGrain : IGrainWithStringKey
    {
        Task StartSymbolsPrice();
    }
}
