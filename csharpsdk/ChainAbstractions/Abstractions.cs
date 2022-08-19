using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChainAbstractions
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
        public Task<ITransaction> Transfer(IFungibleToken token, string recepient, string? memo = null);
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

    public interface IFungibleToken
    {
        ulong Balance { get; }
        IFungibleTokenData Data { get; }
    }

    public enum TransactionState
    {
        Pending,
        Failed,
        PreApproved,
        Approved
    }

    public interface ITransaction
    {
        TransactionState State { get; }
        Error Error { get; }
    }

    public interface ITransferFundsTransaction : ITransaction
    {
        string BalanceFormatted();
        string Memo { get; }
    }

    public interface IBlockchain
    {
        IBasicWallet CreateNewWallet();
        IBasicWallet GetWalletForMnemonic(string mnemonic);
        IBasicWalletInfo? GetWalletInfoForAddress(string address);   
    }
}
