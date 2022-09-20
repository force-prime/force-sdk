using StacksForce.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChainAbstractions
{
    public interface IWalletInfo
    {
        Task<IFungibleToken?> GetToken(string tokenId);

        Task<List<IFungibleToken>> GetAllTokens();

        IDataStream<INFT> GetNFTs(string? nftType = null, bool readMetaData = true);

        public string GetAddress();
    }

    public interface IBasicWallet : IWalletInfo
    {
        public string GetMnemonic();
        public Task<ITransaction> GetTransferTransaction(IFungibleToken token, string recepient, string? memo = null);
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
        Unknown,
        Pending,
        Failed,
        PreApproved,
        Approved
    }

    public interface ITransaction
    {
        IFungibleToken? Cost { get; } 
        TransactionState State { get; }
        Error? Error { get; }

        Task<Error> Send(IFungibleToken? newCost = null);
    }

    public interface ITransferFundsTransaction : ITransaction
    {
        string BalanceFormatted();
        string Memo { get; }
    }

    public interface IBlockchain
    {
        IBasicWallet CreateNewWallet();
        IBasicWallet? GetWalletForMnemonic(string mnemonic);
        IWalletInfo? GetWalletInfoForAddress(string address);
    }
}
