using StacksForce.Stacks;
using StacksForce.Stacks.WebApi;

namespace StacksForceTest
{
    public class WebApiTests
    {
        static WebApiTests()
        {
            StacksDependencies.SetupDefault();
        }

        [Fact]
        public async void GetSTXBalance()
        {
            var result = await Blockchains.Testnet.GetSTXBalance("ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async void GetSTXBalanceIncorrectAccount()
        {
            var result = await Blockchains.Testnet.GetSTXBalance("ST213KNHB5QD308TPG4HS6WJVZ");
            Assert.True(result.IsError && result.Error.Id.Contains("invalid STX address"));
        }

        [Fact]
        public async void GetLastNonce()
        {
            var result = await Blockchains.Testnet.GetLastNonce("ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess && result.Data.last_executed_tx_nonce > 1 && result.Data.possible_next_nonce > result.Data.last_executed_tx_nonce);
        }

        [Fact]
        public async void GetBalances()
        {
            var stxBalance = await Blockchains.Testnet.GetSTXBalance("ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            var result = await Blockchains.Testnet.GetBalances("ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess && stxBalance.Data.balance == result.Data.stx.balance);
        }

        [Fact]
        public async void GetNFTHoldings()
        {
            var result = await Blockchains.Testnet.GetNFTHoldings("ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async void CallReadOnly()
        {
            var result = await Blockchains.Testnet.CallReadOnly("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ", "test", "get-uint", "ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess);
            Assert.True(result.Data.IsOk() && result.Data.UnwrapUntil<Clarity.UInteger128>() != null);

            result = await Blockchains.Testnet.CallReadOnly("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ", "test", "get-ascii", "ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsSuccess && result.Data.IsOk());
            string data = result.Data.UnwrapUntil<Clarity.StringType>().Value;
            Assert.Equal("abcd", data);
        }

        [Fact]
        public async void CallReadOnlyIncorrectMethod()
        {
            var result = await Blockchains.Testnet.CallReadOnly("STVM45V862CRMC3CPE10ZBKRNMCT2Y7KWC20B4EQ", "test", "get-uint-missing", "ST213KNHB5QD308TEESY1ZMX1BP8EZDPG4HS6WJVZ");
            Assert.True(result.IsError && result.Error.Info.Contains("UndefinedFunction"));
        }

        [Fact]
        public async void GetRecentTransactions()
        {
            var result = await Blockchains.Testnet.GetRecentTransactions();
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async void ReadonlyGet()
        {
            var result = await WebApiHelpers.ReadonlyGetString(Blockchains.Mainnet, "SP2P6KSAJ4JVV8PFSNKJ9BNG5PEPR4RT71VXZHWBK", "forcecoin", "get-symbol");
            Assert.True(result.IsSuccess);
            Assert.Equal("FORCE", result.Data);

            result = await WebApiHelpers.ReadonlyGetString(Blockchains.Mainnet, "SP2P6KSAJ4JVV8PFSNKJ9BNG5PEPR4RT71VXZHWBK", "forcecoin", "get-decimals");
            Assert.True(result.IsSuccess);
            Assert.Equal(null, result.Data);

            var result2 = await WebApiHelpers.ReadonlyGetUlong(Blockchains.Mainnet, "SP2P6KSAJ4JVV8PFSNKJ9BNG5PEPR4RT71VXZHWBK", "forcecoin", "get-decimals");
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result2.Data);
        }
    }
}
