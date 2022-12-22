using Orleans;
using Orleans.Runtime;
using StockMarket.Common;
using StockMarket.Common.Models;

namespace StockMarket.SymbolService.Grains
{
    public class UserGrain : Grain, IUserGrain
    {
        private User? _user;
        private IWalletGrain _walletGrain;
        private IUsersGrain _usersGrain;

        
        public override Task OnActivateAsync()
        {
            _user = new User();
            _user.Id = this.GetPrimaryKey();
            _walletGrain = WalletGrainFactory.GetGrain<IWalletGrain>(_user.Id);
            _usersGrain = UsersGrainFactory.GetGrain<IUsersGrain>(0);
            return base.OnActivateAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            _user = user;
            await _usersGrain.AddUser(_user);
            return await Task.FromResult(_user);
        }

        public async Task<List<WalletCurrency>> AddUSDT(double ammount)
        {
            var walletCurrency = new WalletCurrency
            {
                Ammount = ammount,
                Currency = Currency.USDT
            };

            var wallet = await _walletGrain.AddToWallet(walletCurrency);

            return wallet;
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


        /// <summary>
        /// Opens up the grain factory for mocking.
        /// </summary>
        public virtual new IGrainFactory WalletGrainFactory => base.GrainFactory;
        public virtual new IGrainFactory UsersGrainFactory => base.GrainFactory;

        /// <summary>
        /// Opens up the grain key name for mocking.
        /// </summary>
        public virtual string GrainKey => this.GetPrimaryKeyString();

    }
}
