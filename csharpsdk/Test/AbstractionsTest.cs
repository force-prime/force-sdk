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
        }
    }
}
