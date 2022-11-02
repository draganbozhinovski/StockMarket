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

        public async Task<User> CreateUser(string name)
        {
            user.Name = name;
            return await Task.FromResult(user);
        }

        public async Task<List<WalletCurrency>> AddUSDT(double ammount)
        {
            var walletCurrency = new WalletCurrency
            {
                Ammount = ammount,
                Currency = Currency.USDT
            };

           var wallet = await _walletGrain?.AddToWallet(walletCurrency);

            return  wallet;
        }

        public async Task<List<WalletCurrency>> RemoveUsdt(double ammount)
        {
            var walletCurrency = new WalletCurrency
            {
                Ammount = ammount,
                Currency = Currency.USDT
            };

            var wallet = await _walletGrain?.RemoveFromWallet(walletCurrency);

            return wallet;
        }
        public async Task<List<WalletCurrency>> AddToWallet(WalletCurrency walletCurrency)
        {
            var wallet = await _walletGrain?.AddToWallet(walletCurrency);
            return wallet;
        }

        public async Task<List<WalletCurrency>> RemoveFromWallet(WalletCurrency walletCurrency)
        {
            var wallet = await _walletGrain?.RemoveFromWallet(walletCurrency);
            return wallet;
        }

        public async Task<List<WalletCurrency>> GetWallet()
        {
            var wallet = await _walletGrain.GetWallet();
            return wallet;
        }




    }
}
