using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.Metadata;
using System;
using System.Threading.Tasks;
using static ChainAbstractions.Stacks.NFTUtils;

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

        static public Clarity.Value GetNFTId(this INFT nft)
        {
            if (nft is NFT n)
                return n.Id;
            throw new ArgumentException("Incorrect nft argument");
        }

        static public string GetNFTTypeId(this INFT nft)
        {
            if (nft is NFT n)
                return n.AssetTypeId;
            throw new ArgumentException("Incorrect nft argument");
        }

        static public Task<NFTMetaData?> RetrieveMetaData(this INFT nft)
        {
            if (nft is NFT n)
                return n.GetMetaData();
            throw new ArgumentException("Incorrect nft argument");
        }
    }
}
