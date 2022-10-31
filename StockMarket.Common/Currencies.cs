using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public class Currencies
    {
        public List<string>? CurrenciesInUse;
        public Currencies()
        {
            CurrenciesInUse = GetStocks();
        }

        public List<string> GetStocks()
        {
            return Enum.GetNames(typeof(Currency)).ToList();
        }
    }

    public enum Currency
    {
        USDT, BTC, ETH, DOT, ADA, SOL, DOGE, MATIC
    }
}
