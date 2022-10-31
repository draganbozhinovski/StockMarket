using Orleans;
using StockMarket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.SymbolService.Grains
{
    public class WalletGrain : Grain, IWalletGrain
    {
        private List<WalletCurrency>? _walletCurrencies;
        public override Task OnActivateAsync()
        {
            _walletCurrencies = new List<WalletCurrency>();
            return base.OnActivateAsync();
        }
        public Task AddToWallet(WalletCurrency walletCurrency)
        {
            if (_walletCurrencies.Any(x => x.Currency == walletCurrency.Currency))
            {
                return Task.FromResult(() =>
                _walletCurrencies.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount += walletCurrency.Ammount);
            }

            return Task.FromResult(() => _walletCurrencies?.Add(walletCurrency));
        }

        public Task RemoveFromWallet(WalletCurrency walletCurrency)
        {
            if (_walletCurrencies.Any(x => x.Currency == walletCurrency.Currency))
            {
                return Task.FromResult(() =>
                _walletCurrencies.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount -= walletCurrency.Ammount);
            }
            return Task.FromResult(() => _walletCurrencies?.Remove(walletCurrency));
        }

        public Task<List<WalletCurrency>> GetWallet() => Task.FromResult(_walletCurrencies);
    }
}
