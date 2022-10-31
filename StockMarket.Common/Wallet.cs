using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.Common
{
    public class Wallet
    {
        public Guid UserId { get; set; }
        public List<WalletCurrency> Currencies { get; set; }
    }
}
