using Orleans;
using StockMarket.Common;

namespace StockMarket.SymbolService.Grains
{
    public class UserGrain : Grain, IUserGrain
    {
        private User? user;
        private IWalletGrain? _walletGrain;
        public override Task OnActivateAsync()
        {
            user = new User();
            user.Id = this.GetPrimaryKey();
            _walletGrain = GrainFactory.GetGrain<IWalletGrain>(user.Id);
            return base.OnActivateAsync();
        }

        public Task CreateUser(string name)
        {
            return Task.FromResult(() => user.Name = name);
        }

        public Task AddUSDT(double ammount)
        {
            var walletCurrency = new WalletCurrency
            {
                Ammount = ammount,
                Currency = Currency.USDT
            };

            return Task.FromResult(_walletGrain?.AddToWallet(walletCurrency));
        }
        public Task AddToWallet(WalletCurrency walletCurrency)
        {
            return Task.FromResult(_walletGrain?.AddToWallet(walletCurrency));
        }

        public Task RemoveFromWallet(WalletCurrency walletCurrency)
        {
            return Task.FromResult(_walletGrain?.RemoveFromWallet(walletCurrency));
        }

        public Task<List<WalletCurrency>> GetWallet() => Task.FromResult(_walletGrain.GetWallet().Result);




    }
}
