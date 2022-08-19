using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;

namespace StacksForceTest
{
    public class TransactionInfoTests
    {
        static TransactionInfoTests()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public static async void TestTransfer()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Testnet, "5044186ac81109fa864091d9fef8fc69d10e66e9add2a04b528c9d39d8bfbf99");
            var transfer = info as TransferTransactionInfo;
            Assert.NotNull(transfer);
            Assert.Equal("Faucet", transfer.Memo);
            Assert.Equal(500000000, transfer.Amount);
            Assert.Equal(true, transfer.IsAnchored);
            Assert.Equal(TransactionType.TokenTransfer, transfer.Type);
            Assert.Equal(TransactionStatus.Success, transfer.Status);
        }

        [Fact]
        public static async void TestCall()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Testnet, "0xc83b1a9ab6ea2bfce5097172d2e3fce83c43b1e9ba192ce7ee924e2103eeca6e");
            var contractCall = info as ContractCallTransactionInfo;
            Assert.NotNull(contractCall);
            Assert.Equal(true, contractCall.IsAnchored);
            Assert.Equal(TransactionType.ContractCall, contractCall.Type);
            Assert.Equal(TransactionStatus.Success, contractCall.Status);
            Assert.Equal("ST1QK1AZ24R132C0D84EEQ8Y2JDHARDR58SMAYMMW", contractCall.Address);
            Assert.Equal("boom-nfts", contractCall.Contract);
            Assert.Equal("mint-series", contractCall.Function);
            Assert.Equal(contractCall.Arguments[0].ToString(), "STQ9H1T8JT5F6YJWJRK7E98E2D0RNHGA5MKNDJSZ");
            Assert.True(contractCall.Arguments[1].IsNone());
            Assert.True(contractCall.Arguments[2].IsNone());
            Assert.Equal("minee", contractCall.Arguments[3].UnwrapUntil<Clarity.StringType>().Value);
            Assert.Equal("https://boom-nft-develop.s3.amazonaws.com/c55a962a-8f49-4656-ac3d-5169cabf93e2.png", contractCall.Arguments[4].UnwrapUntil<Clarity.StringType>().Value);
            Assert.Equal("image/png", contractCall.Arguments[5].UnwrapUntil<Clarity.StringType>().Value);

            var list = contractCall.Arguments[6] as Clarity.List;
            var tuple = list.Values[0] as Clarity.Tuple;
            Assert.NotNull(tuple);
            Assert.Equal("image/png", tuple.Values["mime-type"].UnwrapUntil<Clarity.StringType>().Value);
            Assert.Equal(1, tuple.Values["number"].UnwrapUntil<Clarity.UInteger128>().Value);

            Assert.True(contractCall.Result.IsOk());

            var resultTuple = contractCall.Result.UnwrapUntil<Clarity.Tuple>();
            Assert.NotNull(resultTuple);
            Assert.Equal(32, resultTuple.Values["series-id"].UnwrapUntil<Clarity.UInteger128>().Value);
        }

        [Fact]
        public static async void TestFailed()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Testnet, "016fe6537fd2a0cd6aea4daf79d766dfe83ce343058e5dd38aef72410484428c");
            var deploy = info as ContractDeployTransactionInfo;
            Assert.NotNull(deploy);
            Assert.Equal(false, deploy.IsAnchored);
            Assert.Equal(TransactionType.SmartContract, deploy.Type);
            Assert.Equal(TransactionStatus.AbortByResponse, deploy.Status);
        }

        [Fact]
        public static async void TestFailed2()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Mainnet, "9ee13c47c9231d64279dd930115271747b6f7bbf4ad3ff1262bec424296fc822");
            var call = info as ContractCallTransactionInfo;
            Assert.NotNull(call);
            Assert.Equal(false, call.IsAnchored);
            Assert.Equal(TransactionType.ContractCall, call.Type);
            Assert.Equal(TransactionStatus.AbortByPostCondition, call.Status);
            Assert.True(call.Result.IsErr());
        }

        [Fact]
        public static async void TestFailed3()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Mainnet, "df4c25a81a94a37d3b2879073416b09ea1888692526e9e09539fb7a2d746e02b");
            var call = info as ContractCallTransactionInfo;
            Assert.NotNull(call);
            Assert.Equal(false, call.IsAnchored);
            Assert.Equal(TransactionType.ContractCall, call.Type);
            Assert.Equal(TransactionStatus.DroppedStaleGarbageCollect, call.Status);
        }
    }
}
