namespace StacksForce.Stacks
{
    public enum ChainID : uint
    {
        Testnet = 0x80000000,
        Mainnet = 0x00000001,
    }

    public enum FungibleConditionCode
    {
        Equal = 0x01,
        Greater = 0x02,
        GreaterEqual = 0x03,
        Less = 0x04,
        LessEqual = 0x05,
    }

    public enum NonFungibleConditionCode
    {
        DoesNotOwn = 0x10,
        Owns = 0x11,
    }

    public enum PayloadType
    {
        TokenTransfer = 0x00,
        SmartContract = 0x01,
        ContractCall = 0x02,
        PoisonMicroblock = 0x03,
        Coinbase = 0x04,
    }

    public enum ClarityType
    {
        Int = 0x00,
        UInt = 0x01,
        Buffer = 0x02,
        BoolTrue = 0x03,
        BoolFalse = 0x04,
        PrincipalStandard = 0x05,
        PrincipalContract = 0x06,
        ResponseOk = 0x07,
        ResponseErr = 0x08,
        OptionalNone = 0x09,
        OptionalSome = 0x0a,
        List = 0x0b,
        Tuple = 0x0c,
        StringASCII = 0x0d,
        StringUTF8 = 0x0e,
    }

    public enum PostConditionPrincipalID
    {
        Origin = 0x01,
        Standard = 0x02,
        Contract = 0x03,
    }

    public enum PostConditionMode
    {
        Allow = 0x01,
        Deny = 0x02,
    }

    public enum PostConditionType
    {
        STX = 0x00,
        Fungible = 0x01,
        NonFungible = 0x02,
    }

    public enum AnchorMode
    {
        /** The transaction MUST be included in an anchored block */
        OnChainOnly = 0x01,
        /** The transaction MUST be included in a microblock */
        OffChainOnly = 0x02,
        /** The leader can choose where to include the transaction (anchored block or microblock)*/
        Any = 0x03,
    }

    public enum StacksMessageType
    {
        Address,
        Principal,
        LengthPrefixedString,
        MemoString,
        AssetInfo,
        PostCondition,
        PublicKey,
        LengthPrefixedList,
        Payload,
        MessageSignature,
        StructuredDataSignature,
        TransactionAuthField,
    }

    public enum AuthType
    {
        Standard = 0x04,
        Sponsored = 0x05,
    }

    public enum AddressVersion
    {
        MainnetSingleSig = 22,
        MainnetMultiSig = 20,
        TestnetSingleSig = 26,
        TestnetMultiSig = 21,
    }

    public enum PubKeyEncoding
    {
        Compressed = 0x00,
        Uncompressed = 0x01,
    }

    public enum AssetType
    {
        STX = 0x00,
        Fungible = 0x01,
        NonFungible = 0x02,
    }

    public enum TransactionVersion
    {
        Mainnet = 0x00,
        Testnet = 0x80,
    }

    public enum AddressHashMode
    {
        // serialization modes for public keys to addresses.
        // We support four different modes due to legacy compatibility with Stacks v1 addresses:
        /** SingleSigHashMode - hash160(public-key), same as bitcoin's p2pkh */
        SerializeP2PKH = 0x00,
        /** MultiSigHashMode - hash160(multisig-redeem-script), same as bitcoin's multisig p2sh */
        SerializeP2SH = 0x01,
        /** SingleSigHashMode - hash160(segwit-program-00(p2pkh)), same as bitcoin's p2sh-p2wpkh */
        SerializeP2WPKH = 0x02,
        /** MultiSigHashMode - hash160(segwit-program-00(public-keys)), same as bitcoin's p2sh-p2wsh */
        SerializeP2WSH = 0x03,
    }

    public enum TransactionType
    {
        TokenTransfer,
        SmartContract,
        ContractCall,
        PoisonMicroblock,
        Coinbase,

        Undefined
    }

    public enum TransactionStatus {
        Pending,
        DroppedReplaceByFee,
        DroppedReplaceAcrossFork,
        DroppedTooExpensive,
        DroppedStaleGarbageCollect,
        Success,
        AbortByResponse,
        AbortByPostCondition,

        Undefined,
    }


    static public class ConstantUtils
    {
        static public AddressVersion? AddressVersionFromByte(byte version) =>
            version switch
            {
                (byte)AddressVersion.MainnetSingleSig => AddressVersion.MainnetSingleSig,
                (byte)AddressVersion.TestnetSingleSig => AddressVersion.TestnetSingleSig,
                (byte)AddressVersion.MainnetMultiSig => AddressVersion.MainnetMultiSig,
                (byte)AddressVersion.TestnetMultiSig => AddressVersion.TestnetSingleSig,
                _ => null
            };
    }
}