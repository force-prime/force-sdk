using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using System;

namespace ChainAbstractions.Stacks
{
    static public partial class StacksAbstractions
    {
        static public Blockchain AsStacksBlockchain(this IBlockchain chain)
        {
            if (chain is BlockchainImplBasic impl)
                return impl._blockchain;
            throw new ArgumentException("Incorrect blockchain argument");
        }

        static public StacksWallet AsStacksWallet(this IBasicWallet wallet)
        {
            if (wallet is BasicWalletImpl impl)
                return impl._wallet;
            throw new ArgumentException("Incorrect wallet argument");
        }

        static public TransactionsManager GetTransactionManager(this IBasicWallet wallet)
        {
            if (wallet is BasicWalletImpl impl)
                return impl._manager;
            throw new ArgumentException("Incorrect wallet argument");
        }

        static public string? GetStacksExplorerLink(this ITransaction transaction)
        {
            if (transaction is TransactionWrapper impl)
                return impl._info != null ? impl._info.StacksExplorerLink : null;
            throw new ArgumentException("Incorrect transaction argument");
        }
    }
}
