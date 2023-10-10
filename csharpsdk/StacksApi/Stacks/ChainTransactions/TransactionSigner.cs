using StacksForce.Utils;

namespace StacksForce.Stacks.ChainTransactions
{
    public class TransactionSigner
    {
        private readonly Transaction _transaction;
        private string _sigHash;

        public string SigHash => _sigHash;

        public TransactionSigner(Transaction transaction)
        {
            _transaction = transaction;
            _sigHash = SignBegin();
        }

        public void SignOrigin(string privateKey)
        {
            _sigHash = SignAndAppend(_transaction.Auth.SpendingCondition, AuthType.Standard, _sigHash, privateKey);
        }

        public void SignSponsor(string privateKey)
        {
            _sigHash = SignAndAppend(_transaction.Auth.SponsorSpendingCondition, AuthType.Sponsored, _sigHash, privateKey);
        }

        private string SignBegin()
        {
            var auth = _transaction.Auth.GetDefault();
            var clone = new Transaction(_transaction.Version, _transaction.ChainId, auth, _transaction.AnchorMode, _transaction.Payload, _transaction.PostConditionMode, _transaction.PostConditions);
            return clone.TxId();
        }

        private string SignAndAppend(SpendingCondition spendingCondition, AuthType authType, string currentSigHash, string privateKey)
        {
            var next = NextSignature(currentSigHash, authType, spendingCondition.Fee, spendingCondition.Nonce, spendingCondition.PublicKey, privateKey);
            spendingCondition.AddSignature(next.sig);
            return next.sigHash;
        }

        static private string MakeSigHashPresign(string curSigHash, AuthType authType, ulong fee, ulong nonce)
        {
            var sigHash = curSigHash + ByteUtils.ByteToHex((byte)authType) +
                ByteUtils.UlongToHex(fee) + ByteUtils.UlongToHex(nonce);

            return SigningUtils.TxIdFromData(sigHash.ToHexByteArray());
        }

        static private (string sig, string sigHash) NextSignature(string curSigHash, AuthType authType, ulong fee, ulong nonce, string publicKey, string privateKey)
        {
            var preSign = MakeSigHashPresign(curSigHash, authType, fee, nonce);
            var signature = SignWithKey(privateKey, preSign);
            var nextSigHash = MakeSigHashPostSign(preSign, publicKey, signature);
            return (signature.ToHex(), nextSigHash);
        }

        static private string MakeSigHashPostSign(string curSigHash, string publicKey, byte[] signature)
        {
            var pubKeyEncoding = SigningUtils.GetPubKeyEncoding(publicKey);

            string keyEnc = ByteUtils.ByteToHex((byte)pubKeyEncoding);
            var sigHash = curSigHash + keyEnc + signature.ToHex();
            return SigningUtils.TxIdFromData(sigHash.ToHexByteArray());
        }

        static private byte[] SignWithKey(string privateKey, string preSign)
        {
            var key = privateKey.ToHexByteArray();
            var data = preSign.ToHexByteArray();
            return SigningUtils.Secp256k1Sign(data, key);
        }
    }
}
