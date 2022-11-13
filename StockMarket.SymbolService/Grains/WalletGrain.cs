using Orleans;
using StockMarket.Common;
using StockMarket.Common.Models;
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
        public async Task<List<WalletCurrency>> AddToWallet(WalletCurrency walletCurrency)
        {
            if (_walletCurrencies.Any(x => x.Currency == walletCurrency.Currency))
            {

                _walletCurrencies.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount += walletCurrency.Ammount;
                return await Task.FromResult(_walletCurrencies);
            }

            _walletCurrencies?.Add(walletCurrency);
            return await Task.FromResult(_walletCurrencies);
        }

        public async Task<List<WalletCurrency>> RemoveFromWallet(WalletCurrency walletCurrency)
        {
            if (_walletCurrencies.Any(x => x.Currency == walletCurrency.Currency))
            {
                _walletCurrencies.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount -= walletCurrency.Ammount;
                return await Task.FromResult(_walletCurrencies);
            }
             _walletCurrencies?.Remove(walletCurrency);
            return await Task.FromResult(_walletCurrencies);
        }

        public async Task<List<WalletCurrency>> GetWallet()
        {
            return await Task.FromResult(_walletCurrencies);
        }
    }
}
