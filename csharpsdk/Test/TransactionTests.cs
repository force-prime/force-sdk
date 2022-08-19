using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;


namespace StacksForceTest
{
    public class TransactionTests
    {
        static TransactionTests()
        {
            StacksDependencies.SetupDefault();
        }

        private const string PUBLIC_KEY = "0490255f88fa311f5dee9425ce33d7d516c24157e2aae8e25a6c631dd6f7322aefcb6ef061bb8ab4c15f4a92b719264fc13c76cdba7f5aa24a4067a5e8405b7b56";
        private const string PRIVATE_KEY = "bcf62fdd286f9b30b2c289cce3189dbf3b502dcd955b2dc4f67d18d77f3e73c7";

        [Fact]
        public static void TestAuthorization()
        {
            const string expected = "04004b153e350d5c841844a7a4681ce26e0ed9936e11000000000000012c0000000003f09d14010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";


            var condition = new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, PUBLIC_KEY);
            condition.UpdateFeeAndNonce(66100500, 300);
            var authorization = new Authorization(AuthType.Standard, condition);

            Assert.Equal(expected, authorization.ToHexString());
        }

        [Fact]
        public static void TestContractCallNoArguments()
        {
            const string expected = "0216df0ba3e79792be7be5e50a370289accfc8c9e0320d636f6e74726163745f6e616d650d66756e6374696f6e5f6e616d6500000000";

            var contractCall = new ContractCallPayload("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", "contract_name", "function_name");

            Assert.Equal(expected, contractCall.ToHexString());
        }

        [Fact]
        public static void TestContractCallWith1Arg()
        {
            const string expected = "0216df0ba3e79792be7be5e50a370289accfc8c9e0320d636f6e74726163745f6e616d650d66756e6374696f6e5f6e616d65000000010100000000000000000000000000038961";


            var contractCall = new ContractCallPayload("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", "contract_name", "function_name",
                  new Clarity.Value[] { new Clarity.UInteger128(231777) });

            Assert.Equal(expected, contractCall.ToHexString());
        }

        [Fact]
        public static void TestContractCallWith2Args()
        {
            const string expected = "0216df0ba3e79792be7be5e50a370289accfc8c9e0320d636f6e74726163745f6e616d650d66756e6374696f6e5f6e616d65000000020100000000000000000000000000038961020000001000112233445566778899aabbccddeeff";

            var contractCall = new ContractCallPayload("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", "contract_name", "function_name",
                new Clarity.Value[] { new Clarity.UInteger128(231777), new Clarity.ByteBuffer("00112233445566778899aabbccddeeff") });
 
            Assert.Equal(expected, contractCall.ToHexString());
        }

        [Fact]
        public static void TestPostCondition()
        {
            const string expected = "000216df0ba3e79792be7be5e50a370289accfc8c9e032030000000038e24b6d";

            var stxPostCondition = new StxPostCondition("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", FungibleConditionCode.GreaterEqual, 954354541);
            Assert.Equal(expected, stxPostCondition.ToHexString());
        }

        [Fact]
        static public void TestDeployTransaction()
        {
            const string expected = "010d636f6e74726163745f6e616d650000014528646566696e652d6d61702073746f72652028286b657920286275666620333229292920282876616c75652028627566662033322929292928646566696e652d7075626c696320286765742d76616c756520286b6579202862756666203332292929202020286d6174636820286d61702d6765743f2073746f72652028286b6579206b657929292920202020202020656e74727920286f6b20286765742076616c756520656e74727929292020202020202028657272203029292928646566696e652d7075626c696320287365742d76616c756520286b65792028627566662033322929202876616c756520286275666620333229292920202028626567696e20202020202020286d61702d7365742073746f72652028286b6579206b6579292920282876616c75652076616c756529292920202020202020286f6b202774727565292929";
            Assert.Equal(expected, CreateTestDeployPayload().ToHexString());
        }

        [Fact]
        static public void TestStxTransferPayload()
        {
            const string expected = "000516df0ba3e79792be7be5e50a370289accfc8c9e03200000000002625a06d656d6f20286e6f74206265696e6720696e636c7564656429000000000000000000";

            var payload = new StxTransferPayload("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", 2500000, "memo (not being included)");

            Assert.Equal(expected, payload.ToHexString());
        }


        [Fact]
        static public void TestTransaction()
        {
            const string expected = "808000000004004b153e350d5c841844a7a4681ce26e0ed9936e11000000000000012c0000000003f09d14010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000030100000001000216df0ba3e79792be7be5e50a370289accfc8c9e032030000000038e24b6d0216df0ba3e79792be7be5e50a370289accfc8c9e0320d636f6e74726163745f6e616d650d66756e6374696f6e5f6e616d65000000020100000000000000000000000000038961020000001000112233445566778899aabbccddeeff";
            var transaction = CreateTestContractCallTransaction();

            Assert.Equal(expected, transaction.ToHexString());
        }

        [Fact]
        static public void TestTransactionTxId()
        {
            const string expected = "bde5c92b87c0c59b8bfa93b9192b18319f0c005b775c7e095b0e3aa97572a1d6";

            var transaction = CreateTestContractCallTransaction();

            Assert.Equal(expected, transaction.TxId());
        }

        [Fact]
        static public void TestSignBegin()
        {
            const string expected = "5df2ccd695a0e1ad994b31970f86bd155afff0b2c5f874f7609bde998cfd00f4";
            var transaction = CreateTestContractCallTransaction();
            var ts = new TransactionSigner(transaction);
            Assert.Equal(expected, ts.SigHash);
        }

        [Fact]
        static public void TestSignOrigin()
        {
            const string expected = "fd6c747ea830f6a200a6c1ef0e34837948d90649cee18850ccef8a78840330bc";
            var transaction = CreateTestContractCallTransaction();
            var ts = new TransactionSigner(transaction);
            ts.SignOrigin(PRIVATE_KEY);
            Assert.Equal(expected, ts.SigHash);
        }

        [Fact]
        static public void TestSignedTransaction()
        {
            const string expected = "808000000004004b153e350d5c841844a7a4681ce26e0ed9936e11000000000000012c0000000003f09d1401009a0a20dda1be802d670866446c83a3a35110ba31943040c944402213b8e6f5d86a10f5dcd78d24990019d0589087082caa0627f171efa59e1a68681d006cd789030100000001000216df0ba3e79792be7be5e50a370289accfc8c9e032030000000038e24b6d0216df0ba3e79792be7be5e50a370289accfc8c9e0320d636f6e74726163745f6e616d650d66756e6374696f6e5f6e616d65000000020100000000000000000000000000038961020000001000112233445566778899aabbccddeeff";
            var transaction = GetSignedTestTransaction();

            Assert.Equal(expected, transaction.ToHexString());
        }

        static private Transaction GetSignedTestTransaction()
        {
            var transaction = CreateTestContractCallTransaction();
            var ts = new TransactionSigner(transaction);
            ts.SignOrigin(PRIVATE_KEY);
            return transaction;
        }

        static private ContractDeployPayload CreateTestDeployPayload()
        {
            var codeBody =
              "(define-map store ((key (buff 32))) ((value (buff 32))))" +
              "(define-public (get-value (key (buff 32)))" +
              "   (match (map-get? store ((key key)))" +
              "       entry (ok (get value entry))" +
              "       (err 0)))" +
              "(define-public (set-value (key (buff 32)) (value (buff 32)))" +
              "   (begin" +
              "       (map-set store ((key key)) ((value value)))" +
              "       (ok 'true)))";

            var contractDeploy = new ContractDeployPayload("contract_name", codeBody);
            return contractDeploy;
        }

        static private Transaction CreateTestContractCallTransaction()
        {

            var condition = new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, PUBLIC_KEY);
            condition.UpdateFeeAndNonce(66100500, 300);
            var authorization = new Authorization(AuthType.Standard, condition);

            var contractCall = new ContractCallPayload("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", "contract_name", "function_name",
               new Clarity.Value[] { new Clarity.UInteger128(231777), new Clarity.ByteBuffer("00112233445566778899aabbccddeeff") });

            var stxPostCondition = new StxPostCondition("SP3FGQ8Z7JY9BWYZ5WM53E0M9NK7WHJF0691NZ159", FungibleConditionCode.GreaterEqual, 954354541);

            var transaction = new Transaction(TransactionVersion.Testnet, ChainID.Testnet, authorization, AnchorMode.Any, contractCall, PostConditionMode.Allow, new PostCondition[] { stxPostCondition });

            return transaction;
        }
    }
}
