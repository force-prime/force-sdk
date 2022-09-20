using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static ChainAbstractions.Stacks.StacksAbstractions;

namespace MauiSample.ViewModels
{
    // Note: INotifyPropertyChanged implementation is handled by Fody.PropertyChanged nuget
    public class WalletViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasWallet { get; set; } = false;
        public string Address { get; set; }
        public string Balance { get; set; }
        public string Mnemonic { get; set; }
        public List<TransactionVM> Transactions { get; set; } = new List<TransactionVM>();


        private IBasicWallet _wallet;

        static private IBlockchain _chain = StacksAbstractions.TestNet;

        public async Task CreateWallet()
        {
            await Task.Run(() =>
            {
                _wallet = _chain.CreateNewWallet();
            });
            Mnemonic = _wallet.GetMnemonic();
            Address = _wallet.GetAddress();
            Balance = "0 STX";
            HasWallet = true;
        }

        public async Task<bool> RestoreWallet(string mnemonic)
        {
            await Task.Run(() =>
            {
                _wallet = _chain.GetWalletForMnemonic(mnemonic);
            });
            if (_wallet != null)
            {
                Mnemonic = mnemonic;
                Address = _wallet.GetAddress();
                var stx = await _wallet.GetToken(null);
                Balance = stx != null ? stx.BalanceFormatted() : "...";
                HasWallet = true;
            }
            else
            {
                HasWallet = false;
            }
            return _wallet != null;
        }

        public async Task<ITransaction> RequestTestStx() {
            var result = await StacksForce.Stacks.WebApi.Faucets.GetSTXTestnetTokens(Blockchains.Testnet, _wallet.GetAddress());

            if (result.IsError)
                return new TransactionWrapper(null, result.Error);

            var info = await TransactionInfo.ForTxId(_chain.AsStacksBlockchain(), result.Data);
            return new TransactionWrapper(info, info != null ? null : new Error("Can't obtain info"));
        }

        public Task<ITransaction> GetTransferStx(string recepient, float amount, string memo) {
            return _wallet.GetTransferTransaction(Stx.From(amount), recepient, memo);
        }

        public Task<Error> SendTransaction(ITransaction transaction)
        {
            return transaction.Send();
        }

        public async Task GetCurrentTransactions()
        {
            var result = await TransactionUtils.GetStxTransactions(_chain.AsStacksBlockchain(), Address);
            if (result.IsSuccess)
            {
                var transactions = result.Data;
                Transactions = transactions.Select(x => new TransactionVM { Id = x.TxId, State = x.Status.ToString(), Info = GetInfoString(x) }).ToList();
            }
        }

        private string GetInfoString(TransferTransactionInfo transactionInfo)
        {
            return $"Transfer({transactionInfo.Memo}) {Stx.FormatCount((ulong) transactionInfo.Amount)} {transactionInfo.Sender} -> {transactionInfo.Recepient} ({transactionInfo.Status})";
        }

        public class TransactionVM : INotifyPropertyChanged
        {

            public event PropertyChangedEventHandler PropertyChanged;

            public string Id { get; set; }
            public string State { get; set; }
            public string Info { get; set; }
        }
    }
}
