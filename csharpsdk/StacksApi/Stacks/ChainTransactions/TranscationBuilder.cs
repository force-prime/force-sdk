namespace StacksForce.Stacks.ChainTransactions
{
    public static class TransactionBuilder
    {
        static public Transaction DeployContract(Blockchain blockchain, StacksAccountBase sender, string contractName, string contractCode, uint fee = 0, uint nonce = 0) {
            var payload = new ContractDeployPayload(contractName, contractCode);

            var auth = GetBasicAuthorization(sender, fee, nonce);
            var transaction = CreateTransaction(blockchain, auth, payload);
            return transaction;

        }

        public static Transaction ContractCall(Blockchain blockchain, StacksAccountBase sender, string address, string contract, string method, Clarity.Value[]? arguments, PostCondition[]? postConditions = null, uint fee = 0, uint nonce = 0)
        {
            var payload = new ContractCallPayload(address, contract, method, arguments);

            var auth = GetBasicAuthorization(sender, fee, nonce);
            var transaction = CreateTransaction(blockchain, auth, payload, postConditions, postConditions == null || postConditions.Length == 0 ? PostConditionMode.Allow : PostConditionMode.Deny);

            return transaction;
        }

        public static Transaction StxTransfer(Blockchain blockchain, StacksAccountBase sender, string principal, ulong amount, string? memo = null, uint fee = 0, uint nonce = 0)
        {
            var payload = new StxTransferPayload(principal, amount, memo);

            var auth = GetBasicAuthorization(sender, fee, nonce);
            var transaction = CreateTransaction(blockchain, auth, payload);
            return transaction;
        }

        public static Transaction GetSponsored(Transaction transaction, StacksAccountBase sponsor, uint sponsorNonce)
        {
            var sponsoredAuth = GetSponsoredAuthorization(transaction.PublicKey, sponsor.PublicKey, transaction.Fee, transaction.Nonce, sponsorNonce);
            return new Transaction(transaction.Version, transaction.ChainId, sponsoredAuth, transaction.AnchorMode, transaction.Payload, transaction.PostConditionMode, transaction.PostConditions);
        }

        static private Transaction CreateTransaction(this Blockchain blockchain, Authorization auth, Payload payload, PostCondition[]? postConditions = null, PostConditionMode postConditionMode = PostConditionMode.Deny)
        {
            return new Transaction(FromChainId(blockchain.ChainId), blockchain.ChainId, auth, AnchorMode.Any, payload, postConditionMode, postConditions ?? new PostCondition[0]);
        }

        static private TransactionVersion FromChainId(ChainID chainId) => chainId switch { ChainID.Mainnet => TransactionVersion.Mainnet, _ => TransactionVersion.Testnet };
	
        static private Authorization GetBasicAuthorization(StacksAccountBase sender, uint fee, uint nonce)
        {
            var condition = new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, sender.PublicKey);
            condition.UpdateFeeAndNonce(fee, nonce);
            var authorization = new Authorization(AuthType.Standard, condition);
            return authorization;
        }

        static private Authorization GetSponsoredAuthorization(string senderPublicKey, string sponsorPublicKey, ulong fee, ulong nonce, uint sponsorNonce)
        {
            var condition = new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, senderPublicKey);
            var sponsorCondition = new SingleSigSpendingCondition(AddressHashMode.SerializeP2PKH, sponsorPublicKey);
            condition.UpdateFeeAndNonce(fee, nonce);
            sponsorCondition.UpdateFeeAndNonce(fee, sponsorNonce);
            var authorization = new Authorization(AuthType.Sponsored, condition, sponsorCondition);
            return authorization;
        }
    }
}
