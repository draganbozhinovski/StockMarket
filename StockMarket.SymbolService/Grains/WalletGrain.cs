using Orleans;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.Grains
{
    public class WalletGrain : Grain, IWalletGrain
    {
        private readonly IPersistentState<List<WalletCurrency>> _wallet;

        public WalletGrain(
            [PersistentState("userWalet", "profileStore")]
            IPersistentState<List<WalletCurrency>> wallet
            )
        {
            _wallet = wallet;
        }

        public async Task<List<WalletCurrency>> AddToWallet(WalletCurrency walletCurrency)
        {
            if (_wallet.State.Any(x => x.Currency == walletCurrency.Currency))
            {

                _wallet.State.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount += walletCurrency.Ammount;
                 await _wallet.WriteStateAsync();
                return await Task.FromResult(_wallet.State);
            }
            _wallet.State.Add(walletCurrency);
            await _wallet.WriteStateAsync();
            return await Task.FromResult(_wallet.State);
        }

        public async Task<List<WalletCurrency>> RemoveFromWallet(WalletCurrency walletCurrency)
        {
            if (_wallet.State.Any(x => x.Currency == walletCurrency.Currency))
            {
                _wallet.State.FirstOrDefault(x => x.Currency == walletCurrency.Currency).Ammount -= walletCurrency.Ammount;
                await _wallet.WriteStateAsync();
                return await Task.FromResult(_wallet.State);
            }
             _wallet.State.Remove(walletCurrency);
            _wallet.WriteStateAsync();
            return await Task.FromResult(_wallet.State);
        }

        public async Task<List<WalletCurrency>> GetWallet()
        {
            return await Task.FromResult(_wallet.State);
        }
    }
}
