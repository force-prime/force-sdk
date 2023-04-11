using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ChainAbstractions
{
    public interface IWalletInfo
    {
        Task<AsyncCallResult<IFungibleToken>> GetToken(string tokenId);

        Task<AsyncCallResult<List<IFungibleToken>>> GetAllTokens();

        IDataStream<INFT> GetNFTs(string? nftType = null, bool readMetaData = true);

        string GetAddress();
    }

    public interface IWallet : IBasicWallet
    {
        string GetMnemonic();
    }

    public interface IBasicWallet : IWalletInfo
    {
        string PublicKey { get; }
        string PrivateKey { get; }

        Task<ITransaction> GetTransferTransaction(IFungibleToken token, string recepient, string? memo = null);
        Task<ITransaction> GetContractCallTransaction(string address, string method, List<IVariable> parameters);
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
        uint Decimals { get; }
    }

    public interface IFungibleToken
    {
        BigInteger Balance { get; }
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
        string Id { get; }
        IFungibleToken? Cost { get; } 
        TransactionState State { get; }
        Error? Error { get; }

        Task<Error> Send(IFungibleToken? newCost = null);
    }

    public interface IContractCallTransaction: ITransaction
    {

    }

    public interface ITransferFundsTransaction : ITransaction
    {
        string BalanceFormatted();
        string Memo { get; }
    }

    public interface IBlockchain
    {
        IWallet CreateNewWallet();
        IWallet? GetWalletForMnemonic(string mnemonic);
        IWalletInfo? GetWalletInfoForAddress(string address);
        IBasicWallet? GetWalletInfoForPrivateKey(string privateKey);

        Task<AsyncCallResult<IVariable>> CallReadOnly(string from, string address, string method, List<IVariable>? parameters = null);
    }

    public interface IVariable
    {
        public enum VariableType
        {
            UnsignedInteger,
            SignedInteger,
            ByteArray,
            Address,
            AsciiString,
            UTF8String,
            Boolean
        }

        VariableType Type { get; }
        T GetValue<T>();
    }

    static public class AbstractionsHelpers
    {
        static public string FormatCount(this IFungibleTokenData data, BigInteger count) => AbstractionsHelpers.FormatBalance(count, data.Decimals, data.Code);

        static public string BalanceFormatted(this IFungibleToken token)
        {
            return token.Data.FormatCount(token.Balance);
        }

        static public string FormatBalance(BigInteger count, uint decimals, string code)
        {
            if (decimals == 0 || count == 0)
                return count.ToString() + " " + code;

            var lowPartSize = (ulong)Math.Pow(10, decimals);
            var mainPart = count / lowPartSize;
            var fracPart = count % lowPartSize;
            return $"{mainPart}.{(fracPart > 0 ? fracPart.ToString("D" + decimals) : "0")} {code}";
        }
    }

    static public class VariableHelpers
    {
        static public IVariable ToVariable(this string value, bool isAscii = false) => new Variable<string>(value, isAscii ? IVariable.VariableType.AsciiString : IVariable.VariableType.UTF8String);
        static public IVariable ToVariable(this BigInteger value, bool signed = true) => new Variable<BigInteger>(value, signed ? IVariable.VariableType.SignedInteger : IVariable.VariableType.UnsignedInteger);
        static public IVariable ToVariable(this long value) => new Variable<BigInteger>(value, IVariable.VariableType.SignedInteger);
        static public IVariable ToVariable(this ulong value) => new Variable<BigInteger>(value, IVariable.VariableType.UnsignedInteger);
        static public IVariable ToVariable(this byte[] value) => new Variable<byte[]>(value, IVariable.VariableType.ByteArray);
        static public IVariable ToAddressVariable(this string value) => new Variable<string>(value, IVariable.VariableType.Address);
        static public IVariable ToAddressVariable(this bool value) => new Variable<bool>(value, IVariable.VariableType.Boolean);
    }

    public class Variable<T> : IVariable
    {
        private T _value;

        public Variable(T value, IVariable.VariableType type)
        {
            Type = type;
            _value = value;
        }

        public T Value => _value;
        public IVariable.VariableType Type { get; private set; }

        public virtual T2 GetValue<T2>()
        {
            if (typeof(T2) == typeof(string))
            {
                return (T2)Convert.ChangeType(_value.ToString(), typeof(T2));
            }
            return (T2)(object)_value;
            // return (T2)Convert.ChangeType(_value, typeof(T2));
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
