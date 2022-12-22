using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common.Models
{
    [Serializable]
    public class WalletCurrency
    {
        public Currency Currency { get; set; }
        public double Ammount { get; set; }
    }
}
