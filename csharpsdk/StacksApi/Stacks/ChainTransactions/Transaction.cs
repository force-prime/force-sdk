using StacksForce.Utils;
using System;
using System.IO;
using System.Text;

namespace StacksForce.Stacks.ChainTransactions
{
    public sealed class Transaction : IBinarySerializable
    {
        public TransactionVersion Version { get; }
        public ChainID ChainId { get; }
        public Authorization Auth { get;  }
        public AnchorMode AnchorMode { get; }
        public Payload Payload { get; }
        public PostConditionMode PostConditionMode { get; }
        public PostCondition[] PostConditions { get; }

        public ulong Nonce => Auth.SpendingCondition.Nonce;
        public ulong Fee => Auth.SpendingCondition.Fee;

        public string PublicKey => Auth.SpendingCondition.PublicKey;

        public Transaction(TransactionVersion version, ChainID chainId, Authorization auth, AnchorMode anchorMode, Payload payload, PostConditionMode postConditionMode, PostCondition[] postConditions)
        {
            Version = version;
            ChainId = chainId;
            Auth = auth;
            AnchorMode = anchorMode;
            Payload = payload;
            PostConditionMode = postConditionMode;
            PostConditions = postConditions;
        }

        public string TxId()
        {
            return SigningUtils.TxIdFromData(Serialize());
        }

        public TransactionType TransactionType => Payload.Type switch
        {
            PayloadType.SmartContract => TransactionType.SmartContract,
            PayloadType.ContractCall => TransactionType.ContractCall,
            PayloadType.TokenTransfer => TransactionType.TokenTransfer
        };

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)Version);
                writer.Write(ByteUtils.UInt32ToByteArrayBigEndian((uint) ChainId));
                Auth.SerializeTo(writer);
                writer.Write((byte)AnchorMode);
                writer.Write((byte)PostConditionMode);
                SerializationUtils.SerializeLPList(PostConditions, writer);
                Payload.SerializeTo(writer);
            }
            byte[] bytes = stream.ToArray();
            return bytes;
        }

        public void UpdateFeeAndNonce(ulong fee, ulong nonce)
        {
            if (Auth.SponsorSpendingCondition != null)
                Auth.SponsorSpendingCondition.UpdateFeeAndNonce(fee, Auth.SponsorSpendingCondition.Nonce);

            Auth.SpendingCondition.UpdateFeeAndNonce(fee, nonce);
        }

        public void SerializeTo(BinaryWriter writer)
        {
            writer.Write(Serialize());
        }
    }

    public abstract class SpendingCondition : IBinarySerializable
    {
        public AddressHashMode HashMode { get; }
        public string PublicKey { get; }
        public ulong Nonce { get; private set; } = 0;
        public ulong Fee { get; private set; } = 0;

        protected SpendingCondition(AddressHashMode hashMode, string publicKey)
        {
            HashMode = hashMode;
            PublicKey = publicKey;
        }

        public virtual void SerializeTo(BinaryWriter writer)
        {
            writer.Write((byte)HashMode);
            writer.Write(string.IsNullOrEmpty(PublicKey) ? SigningUtils.ZERO_BYTES_20 : Address.AddressFromPublicKey(HashMode, PublicKey));
            writer.Write(ByteUtils.UInt64ToByteArrayBigEndian(Nonce));
            writer.Write(ByteUtils.UInt64ToByteArrayBigEndian(Fee));
        }

        public void UpdateFeeAndNonce(ulong fee, ulong nonce)
        {
            Fee = fee;
            Nonce = nonce;
        }

        public abstract SpendingCondition GetDefault();

        public abstract void AddSignature(string signature);
        public abstract bool IsSigned();
    }

    public sealed class SingleSigSpendingCondition : SpendingCondition
    {
        public byte[] Signature { get; private set; } = SigningUtils.EMPTY_SIG_65;

        public SingleSigSpendingCondition(AddressHashMode hashMode, string publicKey) : base(hashMode, publicKey)
        {
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);

            writer.Write((byte)SigningUtils.GetPubKeyEncoding(PublicKey));
            writer.Write(Signature);
        }

        public override SpendingCondition GetDefault()
        {
            return new SingleSigSpendingCondition(HashMode, PublicKey);
        }

        public override void AddSignature(string signature)
        {
            Signature = signature.ToHexByteArray();
        }

        static public SingleSigSpendingCondition GetSigningSentinel()
        {
            return new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, string.Empty);
        }

        public override bool IsSigned() => Signature != SigningUtils.EMPTY_SIG_65;
    }

    public sealed class Authorization : IBinarySerializable
    {
        public AuthType AuthType { get; }
        public SpendingCondition SpendingCondition { get; }
        public SingleSigSpendingCondition? SponsorSpendingCondition { get; }

        public Authorization(AuthType authType, SpendingCondition spendingCondition, SingleSigSpendingCondition? sponsorSpendingCondition = null)
        {
            if (authType == AuthType.Standard && sponsorSpendingCondition != null)
                throw new ArgumentException("Incorrect number of spending conditions");
            if (authType == AuthType.Sponsored && sponsorSpendingCondition == null)
                throw new ArgumentException("Incorrect number of spending conditions");

            AuthType = authType;
            SpendingCondition = spendingCondition;
            SponsorSpendingCondition = sponsorSpendingCondition;
        }

        public void SerializeTo(BinaryWriter writer)
        {
            writer.Write((byte)AuthType);
            SpendingCondition.SerializeTo(writer);
            SponsorSpendingCondition?.SerializeTo(writer);
        }
        public Authorization GetDefault()
        {
            if (SponsorSpendingCondition != null)
                return new Authorization(AuthType, SpendingCondition.GetDefault(), SingleSigSpendingCondition.GetSigningSentinel());
            return new Authorization(AuthType, SpendingCondition.GetDefault());
        }

        public bool IsSigned() => SpendingCondition.IsSigned() && (SponsorSpendingCondition == null || SponsorSpendingCondition.IsSigned());
    }

    public abstract class PostCondition : IBinarySerializable
    {
        private PostConditionType _type;

        public PostCondition(PostConditionType type)
        {
            _type = type;
        }

        public PostConditionType Type => _type;

        public virtual void SerializeTo(BinaryWriter writer)
        {
            writer.Write((byte)_type);
        }

        protected void WritePrincipal(BinaryWriter writer, string principal)
        {
            writer.Write((byte)GetPrincipalType(principal));
            writer.Write(SerializationUtils.SerializeAddress(principal));
        }

        static private PostConditionPrincipalID GetPrincipalType(string principal)
        {
            return PostConditionPrincipalID.Standard;
        }
    }

    public sealed class StxPostCondition : PostCondition
    {
        public string Principal { get; }
        public FungibleConditionCode Condition { get; }
        public ulong Value { get; }

        public StxPostCondition(string principal, FungibleConditionCode condition, ulong value) : base(PostConditionType.STX)
        {
            Principal = principal;
            Condition = condition;
            Value = value;
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            WritePrincipal(writer, Principal);
            writer.Write((byte)Condition);
            writer.Write(ByteUtils.UInt64ToByteArrayBigEndian(Value));
        }
    }

    public sealed class FungibleTokenPostCondition : PostCondition
    {
        public string Principal { get; }
        public FungibleConditionCode Condition { get; }
        public ulong Value { get; }

        public AssetInfo Asset { get; }

        public FungibleTokenPostCondition(string principal, AssetInfo asset, FungibleConditionCode condition, ulong value) : base(PostConditionType.Fungible)
        {
            Principal = principal;
            Asset = asset;
            Condition = condition;
            Value = value;
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            WritePrincipal(writer, Principal);
            Asset.SerializeTo(writer);
            writer.Write((byte)Condition);
            writer.Write(ByteUtils.UInt64ToByteArrayBigEndian(Value));
        }
    }

    public sealed class NFTPostCondition : PostCondition
    {
        public string Principal { get; }
        public NonFungibleConditionCode Condition { get; }

        public AssetInfo Asset { get; }

        public Clarity.Value AssetId { get; }

        public NFTPostCondition(string principal, AssetInfo asset, NonFungibleConditionCode condition, Clarity.Value assetId) : base(PostConditionType.NonFungible)
        {
            Principal = principal;
            Asset = asset;
            Condition = condition;
            AssetId = assetId;  
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            WritePrincipal(writer, Principal);
            Asset.SerializeTo(writer);
            AssetId.SerializeTo(writer);
            writer.Write((byte)Condition);
        }
    }

    public class AssetInfo : IBinarySerializable
    {
        public string Address { get; }
        public string Contract { get; }
        public string Asset { get; }

        public AssetInfo(string address, string contract, string asset)
        {
            Address = address;
            Contract = contract;
            Asset = asset;
        }

        public AssetInfo(string fullAssetId)
        {
            var parsed = Stacks.Address.ParseFromFullTokenId(fullAssetId);
            Address = parsed.address;
            Contract = parsed.contract;
            Asset = parsed.token;
        }

        public void SerializeTo(BinaryWriter writer)
        {
            writer.Write(SerializationUtils.SerializeAddress(Address));
            writer.Write(SerializationUtils.SerializeLPString(Contract));
            writer.Write(SerializationUtils.SerializeLPString(Asset));
        }
    }

    public abstract class Payload : IBinarySerializable
    {
        private readonly PayloadType _payloadType;

        protected Payload(PayloadType payloadType) { _payloadType = payloadType; }

        public PayloadType Type => _payloadType;
        public virtual void SerializeTo(BinaryWriter writer)
        {
            writer.Write((byte)_payloadType);
        }
    }

    public sealed class StxTransferPayload : Payload
    {
        private const int MEMO_MAX_LENGTH_BYTES = 34;

        public string Principal { get; }
        public ulong Amount { get; }
        public string? Memo { get; }

        public StxTransferPayload(string principal, ulong amount, string? memo = null) : base(PayloadType.TokenTransfer)
        {
            Principal = principal;
            Amount = amount;
            Memo = memo;

            if (Memo != null && Memo.Length > MEMO_MAX_LENGTH_BYTES)
                throw new ArgumentException("Memo is too long");
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            Clarity.Principal.FromString(Principal).SerializeTo(writer);
            writer.Write(ByteUtils.UInt64ToByteArrayBigEndian(Amount));

            var s = Memo ?? string.Empty;
            writer.Write(Encoding.ASCII.GetBytes(s).PadRight(MEMO_MAX_LENGTH_BYTES));
        }
    } 

    public sealed class ContractDeployPayload : Payload
    {
        public string ContractName;
        public string CodeBody;

        public ContractDeployPayload(string contractName, string codeBody) : base(PayloadType.SmartContract)
        {
            ContractName = contractName;
            CodeBody = codeBody;
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            writer.Write(SerializationUtils.SerializeLPString(ContractName));
            writer.Write(SerializationUtils.SerializeLPString(CodeBody, 4));
        }
    }

    public sealed class ContractCallPayload : Payload
    {
        public string ContractAddress { get; }
        public string ContractName { get; }
        public string FunctionName { get; }

        public Clarity.Value[] Arguments { get; }

        public ContractCallPayload(string contractAddress, string contractName, string functionName, Clarity.Value[]? arguments = null) : base(PayloadType.ContractCall)
        {
            ContractAddress = contractAddress;
            ContractName = contractName;
            FunctionName = functionName;
            Arguments = arguments ?? new Clarity.Value[0];
        }

        public override void SerializeTo(BinaryWriter writer)
        {
            base.SerializeTo(writer);
            writer.Write(SerializationUtils.SerializeAddress(ContractAddress));
            writer.Write(SerializationUtils.SerializeLPString(ContractName));
            writer.Write(SerializationUtils.SerializeLPString(FunctionName));
            SerializationUtils.SerializeLPList(Arguments, writer);
        }
    }
}
