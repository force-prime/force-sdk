using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;

namespace ChainAbstractions.Stacks
{
    static public partial class StacksAbstractions
    {
        static public Blockchain? AsStacksBlockchain(this IBlockchain chain) => (chain as BlockchainImplBasic)?._blockchain;
        static public StacksWallet? AsStacksWallet(this IBasicWallet abstractWallet) => (abstractWallet as BasicWalletImpl)?._wallet;
        static public TransactionsManager? GetTransactionManager(this IBasicWallet abstractWallet) => (abstractWallet as BasicWalletImpl)?._manager;
    }
}
