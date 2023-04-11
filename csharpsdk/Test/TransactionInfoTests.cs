using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;
using StacksForce.Utils;

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
            var transfer = info.Data as TransferTransactionInfo;
            Assert.NotNull(transfer);
            Assert.Equal("Faucet", transfer.Memo);
            Assert.Equal(500000000, transfer.Amount);
            Assert.True(transfer.IsAnchored);
            Assert.Equal(TransactionType.TokenTransfer, transfer.Type);
            Assert.Equal(TransactionStatus.Success, transfer.Status);
        }

        [Fact]
        public static async void TestCall()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Testnet, "0xc83b1a9ab6ea2bfce5097172d2e3fce83c43b1e9ba192ce7ee924e2103eeca6e");
            var contractCall = info.Data as ContractCallTransactionInfo;
            Assert.NotNull(contractCall);
            Assert.True(contractCall.IsAnchored);
            Assert.Equal(TransactionType.ContractCall, contractCall.Type);
            Assert.Equal(TransactionStatus.Success, contractCall.Status);
            Assert.Equal("ST1QK1AZ24R132C0D84EEQ8Y2JDHARDR58SMAYMMW", contractCall.Address);
            Assert.Equal("boom-nfts", contractCall.Contract);
            Assert.Equal("mint-series", contractCall.Function);
            Assert.Equal("STQ9H1T8JT5F6YJWJRK7E98E2D0RNHGA5MKNDJSZ", contractCall.Arguments[0].ToString());
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
            var deploy = info.Data as ContractDeployTransactionInfo;
            Assert.NotNull(deploy);
            Assert.False(deploy.IsAnchored);
            Assert.Equal(TransactionType.SmartContract, deploy.Type);
            Assert.Equal(TransactionStatus.AbortByResponse, deploy.Status);
        }

        [Fact]
        public static async void TestFailed2()
        {
            var info = await TransactionInfo.ForTxId(Blockchains.Mainnet, "9ee13c47c9231d64279dd930115271747b6f7bbf4ad3ff1262bec424296fc822");
            var call = info.Data as ContractCallTransactionInfo;
            Assert.NotNull(call);
            Assert.False(call.IsAnchored);
            Assert.Equal(TransactionType.ContractCall, call.Type);
            Assert.Equal(TransactionStatus.AbortByPostCondition, call.Status);
            Assert.True(call.Result.IsErr());
        }

        [Fact]
        public static async void TestEvents()
        {
            var infoResult = await TransactionInfo.ForTxId(Blockchains.Mainnet, "0xe2355eaec9795effe58be9d1ccffd939e1799b0c07b5787ab5911f43f41765cf");
            var info = infoResult.Data;
            Assert.NotNull(info);
            var s1 = info.Events.GetStream();
            var s1Task = s1.ReadMoreAsync(1);
            var s2 = info.Events.GetStream();
            s2.ReadMoreAsync(2);
            var s3 = info.Events.GetStream();
            s3.ReadMoreAsync(3);
            var s4 = info.Events.GetStream();
            await Task.Delay(1000);
            await s1Task;
            var first = await s4.ReadMoreAsync(1);
            Assert.Equal("SP3K8BC0PPEVCV7NZ6QSRWPQ2JE9E5B6N3PA0KBR9.age000-governance-token::alex", (first[0] as FTEvent).AssetId);
            Assert.Equal(187911124ul, (first[0] as FTEvent).Amount);
            Assert.Equal(TransactionEvent.TokenEventType.Mint, (first[0] as FTEvent).Type);
            var last = await s1.ReadMoreAsync(20);
            Assert.Equal(1, first.Count);
            Assert.Equal(20, last.Count);
            var last2 = await s1.ReadMoreAsync(25);
            Assert.Equal(25, last2.Count);

            Assert.Equal("SP3K8BC0PPEVCV7NZ6QSRWPQ2JE9E5B6N3PA0KBR9.token-apower::apower", (last2[24] as FTEvent).AssetId);
            Assert.Equal(30368232ul, (last2[24] as FTEvent).Amount);
            Assert.Equal(TransactionEvent.TokenEventType.Mint, (last2[24] as FTEvent).Type);

        }

        [Fact]
        public async void TestBlockTransationsStream()
        {
            List<IDataStream<TransactionInfo>> sources = new List<IDataStream<TransactionInfo>>();

            for (int b = 95581; b <= 95584; b++)
                sources.Add(new BlockTransactionsStream(Blockchains.Testnet, (uint)b));

            var s = new MultipleSourcesDataStream<TransactionInfo>(sources);
            var result = await s.ReadMoreAsync(20);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal("620e5d7a09ed8ba6bafc050f93cc8c83da8af48d3a86ddbabdb511f7cfaf0d64", result[0].TxId);
            Assert.Equal("0100d6a1b16816927664ee262ef95fce32930e7c21b2a2f24a87244fb63a6f2d", result[4].TxId);
            Assert.Equal("164701b3d2f6804342af974d1073ce7ec76eab2a573bd41fe40345273ef384c2", result[9].TxId);
        }
    }
}
