using ChainAbstractions;
using ChainAbstractions.Stacks;
using ChainAbstractions.Stacks.ContractWrappers;
using StacksForce.Stacks;

namespace StacksForceTest
{
    public class AbstractionsTest
    {
        static AbstractionsTest()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public async void TestWalletInfo()
        {
            var walletInfo = StacksAbstractions.TestNet.GetWalletInfoForAddress("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ");
            var stx = await walletInfo.GetToken(null).ConfigureAwait(false);
            Assert.True(stx.IsSuccess);
            Assert.Equal("499.980036 STX", stx.Data.BalanceFormatted());
            var nftStream = walletInfo.GetNFTs(null);
            var nfts = await nftStream.ReadMoreAsync(1).ConfigureAwait(false);
            Assert.Equal("TEST-NFT", nfts[0].Name);
        }

        [Fact]
        public async void TestSIP10()
        {
            var sip10 = new SIP10("ST2SDZYR4VQF138X2A76KEFFRC6A834MDXXWFRZW1", "basic-token", "GAME-CURRENCY");
            var nameResult = await sip10.GetName().ConfigureAwait(false);
            Assert.Equal("GAME-CURRENCY", nameResult.Data);

            var symbolResult = await sip10.GetSymbol().ConfigureAwait(false);
            Assert.Equal("GACU", symbolResult.Data);

            var decimalsResult = await sip10.GetDecimals().ConfigureAwait(false);
            Assert.Equal((uint) 0, decimalsResult.Data);

            var uriResult = await sip10.GetTokenUri().ConfigureAwait(false);
            Assert.True(uriResult.IsSuccess);
        }

        [Fact]
        public async void TestSIP09()
        {
            var sip09 = new SIP09UnsignedInteger("SP2KAF9RF86PVX3NEE27DFV1CQX0T4WGR41X3S45C", "magic-ape-school", "magic-ape-school");
            var tokenUri = await sip09.GetTokenUri(1);
            Assert.Equal("ipfs://QmfXpeb2dci371W4REDYMaRVVwoDx4BJmVzr9igsrbK62T/1.json", tokenUri.Data);

            var ownerResult = await sip09.GetTokenOwner(1);
            Assert.True(ownerResult.IsSuccess);

            var nft = await sip09.GetById(1);
            Assert.NotNull(nft);
            Assert.Equal("Magic Ape #1", nft.Name);
            Assert.Equal("Primary school magicians are neophytes who only comprehend the world of magic, but you should not neglect them, because even among these first graders there are valuable nuggets!", nft.Description);
            Assert.Equal("Magic Ape School", nft.Collection);
            Assert.Equal("https://byzantion.mypinata.cloud/ipfs/Qma2n67Ab853cFf1YAHWyjdBVjzb6ZFz1h3doSh4pnhgDf/1.png", nft.ImageUrl);
        }

        [Fact]
        public async void TestTransactionInfo()
        {
            var txInfo = await StacksAbstractions.TestNet.GetTransactionInfo("0xa0fd6524416483003f8765989a29ed9c1007fa137f85d7646f82a45da713278f");
            Assert.True(txInfo.IsSuccess);
            Assert.Equal(TransactionState.Approved, txInfo.Data.State);
            Assert.Equal(900, txInfo.Data.Cost.Balance);
            Assert.Equal(544ul, txInfo.Data.Nonce);

            var txInfo2 = await StacksAbstractions.MainNet.GetTransactionInfo("11e7c97ebadd1eb9f7bddc33dd218ef8e854cf677dab199e1e8050a69a443a7b");
            Assert.True(txInfo2.IsSuccess);
            Assert.Equal(TransactionState.Failed, txInfo2.Data.State);
            Assert.Equal(311593, txInfo2.Data.Cost.Balance);
            Assert.Equal(7ul, txInfo2.Data.Nonce);

            var txInfo3 = await StacksAbstractions.MainNet.GetTransactionInfo("wrong");
            Assert.True(txInfo3.IsError);
        }
    }
}
