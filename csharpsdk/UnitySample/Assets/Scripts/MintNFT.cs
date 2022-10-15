using ChainAbstractions.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks;
using ChainAbstractions;
using System.Threading.Tasks;
using System.Buffers.Binary;
using StacksForce.Utils;
using System.Security.Cryptography;

static public class MintNFT 
{
    static public string NFT_ID = $"{CONTRACT_ADDRESS}.{CONTRACT_NAME}::{NFT_NAME}";

    private const string CONTRACT_ADDRESS = "SP137KE5TWH59D3N53KD5MXM4TZP2FPQD2VCTXV43";
    private const string CONTRACT_NAME = "flappy-result-nft-v3";
    private const string NFT_NAME = "FLAPPY-RESULT-NFT";
    private const string METHOD_NAME = "mint";

    static public async Task<ITransaction> GetMintTransaction(IBasicWallet wallet, int score)
    {
        // TODO: get correct values from server
        // StacksForce.Dependencies.DependencyProvider.HttpClient.Get()

        byte[] values = new byte[64];
        byte[] signature = new byte[64];

        var manager = wallet.GetTransactionManager();
        var result = await manager.GetContractCall(CONTRACT_ADDRESS, CONTRACT_NAME, METHOD_NAME,
            new Clarity.Value[] {
                new Clarity.ByteBuffer(values),
                new Clarity.ByteBuffer(signature),
            },
            new PostCondition[] {});

        return new StacksAbstractions.TransactionWrapper(manager, result);
    }
}
