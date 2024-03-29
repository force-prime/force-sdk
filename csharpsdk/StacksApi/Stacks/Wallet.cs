﻿using StacksForce.Dependencies;
using StacksForce.Utils;
using System.Text;

namespace StacksForce.Stacks
{
    public class StacksWallet
    {
        private const string DATA_DERIVATION_PATH = "m/888'/0'";
        private const string WALLET_CONFIG_PATH = "m/44/5757'/0'/1";
        private const string STX_DERIVATION_PATH = "m/44'/5757'/0'/0";
        private const string BTC_DERIVATION_PATH = "m/84'/0'/";

        internal const uint HARDENED_OFFSET = 0x80000000;

        private readonly IHDKey _rootNode;
        private readonly string _salt;
        private readonly string _mnemonic;

        public string Salt => _salt;
        public string Mnemonic => _mnemonic;

        static public string GenerateMnemonicPhrase() => DependencyProvider.BtcFeatures.GenerateMnemonicPhrase();
        
        public StacksWallet(string mnemonic, string password = "")
        {
            var seed = DependencyProvider.BtcFeatures.MnemonicToSeedHex(mnemonic, password);
            _rootNode = DependencyProvider.BtcFeatures.GetFromSeed(seed);
            _mnemonic = mnemonic;
            _salt = DeriveSalt(_rootNode);
        }

        public StacksAccount GetAccount(uint index) {
            var acc = DeriveAccount(_rootNode, index);

            return new StacksAccount(acc.privateKey, acc.appsNode, _salt);
        }

        public BitcoinAccountBase GetBTCAccount(uint index)
        {
            var acc = _rootNode.Derive(BTC_DERIVATION_PATH).Derive(index + HARDENED_OFFSET);
            acc = acc.Derive(0).Derive(0);
            return new BitcoinAccountBase(acc.PrivateKey.ToHex(), acc.PublicKey.ToHex());
        }

        static private (string privateKey, Dependencies.IHDKey appsNode) DeriveAccount(Dependencies.IHDKey rootNode, uint index)
        {
            var stxPrivateKey = DeriveStxPrivateKey(rootNode, index);
            var identitiesKeychain = rootNode.Derive(DATA_DERIVATION_PATH);
            var identityKeychain = identitiesKeychain.Derive(index + HARDENED_OFFSET);

            var appsNode = identityKeychain.Derive(0 + HARDENED_OFFSET);

            return (stxPrivateKey, appsNode);
        }

        static private string DeriveSalt(Dependencies.IHDKey hdKey)
        {
            Dependencies.IHDKey derived = hdKey.Derive(DATA_DERIVATION_PATH);
            byte[] key = derived.PublicKey;
            var b = Encoding.ASCII.GetBytes(key.ToHex());
            var saltData = SigningUtils.Sha256(b);
            return saltData.ToHex();
        }

        private static string DeriveStxPrivateKey(Dependencies.IHDKey rootNode, uint index)
        {
            var childKey = rootNode.Derive(STX_DERIVATION_PATH).Derive(index);
            var compressedKey = SigningUtils.CompressPrivateKey(childKey.PrivateKey);
            return compressedKey.ToHex();
        }
    }

    public class BitcoinAccountBase
    {
        private readonly string _privateKey;
        private readonly string _publicKey;

        public string PublicKey => _publicKey;
        public string PrivateKey => _privateKey;

        public BitcoinAccountBase(string privateKey, string publicKey)
        {
            _privateKey = privateKey;
            _publicKey = publicKey;
        }

        public string GetBtcSegwitAddress()
        {
            return DependencyProvider.BtcFeatures.GetSegwitAddress(_publicKey)!;
        }
    }

    public class StacksAccountBase
    {
        private readonly string _privateKey;
        private readonly string _publicKey;

        public string PublicKey => _publicKey;
        public string PrivateKey => _privateKey;

        public StacksAccountBase(string privateKey)
        {
            _privateKey = privateKey;
            _publicKey = SigningUtils.GetPublicKeyFromPrivateKey(_privateKey);
        }

        public string GetAddress(AddressVersion addressVersion)
        {
            return Address.AddressFromPublicKey(addressVersion, AddressHashMode.SerializeP2PKH, _publicKey);
        }
    }

    public class StacksAccount : StacksAccountBase
    {
        private readonly IHDKey _appsKey;
        private readonly string _salt;

        public StacksAccount(string privateKey, IHDKey appsNode, string salt) : base(privateKey)
        {
            _appsKey = appsNode;
            _salt = salt;
        }

        public string GetAppPrivateKey(string appDomain)
        {
            var hashBuffer = SigningUtils.Sha256(Encoding.ASCII.GetBytes(appDomain + _salt));
            var appIndex = SigningUtils.GetStringHashCode(hashBuffer.ToHex());
            var appKeychain = _appsKey.Derive(appIndex + StacksWallet.HARDENED_OFFSET);
            return appKeychain.PrivateKey.ToHex();
        }
    }
}
