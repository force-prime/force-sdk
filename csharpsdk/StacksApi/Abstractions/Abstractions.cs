using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Abstractions
{
    public interface IBasicWalletInfo
    {
        Task<IFungibleToken> GetToken(string currency);

        Task<List<IFungibleToken>> GetAllTokens();

        IDataStream<INFT> GetNFTs(string nftType);

        public string GetAddress();
    }

    public interface IBasicWallet : IBasicWalletInfo
    {
        public string GetMnemonic();
    }

    public interface INFT
    {
        string Name { get; }
        string Description { get; }
        string ImageUrl { get; } 
    }

    public interface IFungibleTokenData {
        string Code { get; }
        string Description { get; }
        string ImageUrl { get; }
        string FormatCount(ulong count);
    }

    public interface IFungibleToken : IFungibleTokenData
    {
        ulong Balance { get; }
    }

    public enum TransactionState
    {
        Pending,
        Canceled,
        PreApproved,
        Approved
    }

    public interface ITransaction
    {
        TransactionState State { get; }
    }

    public interface ITransferFundsTransaction : ITransaction
    {
        string BalanceFormatted();
        string Memo { get; }
    }

    public interface IBlockchain
    {
        IBasicWallet CreateNewWallet();
        IBasicWalletInfo? GetWalletInfoForAddress(string address);   
    }
}
