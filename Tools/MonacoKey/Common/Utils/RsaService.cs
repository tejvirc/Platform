namespace Common.Utils
{
    using Common.Models;
    using log4net;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    // Shoutouts
    // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=netframework-4.8
    // https://www.bouncycastle.org/csharp/index.html
    // https://en.wikipedia.org/wiki/Privacy-Enhanced_Mail
    public class RsaService : NotifyPropertyChanged
    {
        private ObservableCollection<RSAKeyPair> _keyPairs = new ObservableCollection<RSAKeyPair> { };
        private ILog _log;
        private RSAKeyPair _selectedKeyPair = null;

        private bool _requirePrivateKey = true;
        private bool _requirePublicKey = true;
        private SHA256CryptoServiceProvider _shaProvider = new SHA256CryptoServiceProvider();

        public RsaService(ILog log, bool requirePrivateKey = true, bool requirePublicKey = true)
        {
            _requirePrivateKey = requirePrivateKey;
            _requirePublicKey = requirePublicKey;
            _log = log;

            // Ideally there shouldn't be any IO in a constructor like this, but oh well, this makes it easy to code, and I don't see any performance issues.
            LoadKeyPairsFromDefaultLocations();
            SelectDefaultKeyPair();
        }
        public RsaService(ILog log, string publicKeyFullPath, string keyPairName)
        {
            _requirePrivateKey = false;
            _requirePublicKey = true;
            _log = log;

            // Ideally there shouldn't be any IO in a constructor like this, but oh well, this makes it easy to code, and I don't see any performance issues.
            try
            {
                string publicKeyFileName = Path.GetFileName(publicKeyFullPath);
                string publicKeyDirectoryName = Path.GetDirectoryName(publicKeyFullPath);
                LoadKeyPairFromPath(publicKeyDirectoryName, keyPairName, publicKeyFileName, "fileDoesNotMatterHere.rsa");
            }
            catch(Exception e)
            {
                // user probably used incorrect path in command line params
            }
            SelectDefaultKeyPair();
        }

        public ObservableCollection<RSAKeyPair> KeyPairs
        {
            get
            {
                return _keyPairs;
            }
            set
            {
                _keyPairs = value;
                OnPropertyChanged(nameof(KeyPairs));
            }
        }
        public RSAKeyPair SelectedKeyPair
        {
            get
            {
                return _selectedKeyPair;
            }
            set
            {
                _selectedKeyPair = value;
                OnPropertyChanged(nameof(SelectedKeyPair));
            }
        }

        private void LoadKeyPairsFromDefaultLocations()
        {
            string keysDirectoryPath = @"Keys/";

            LoadAllSubDirectoryKeys(keysDirectoryPath);
        }
        private bool LoadKeyPairFromPath(string directoryPath, string keyPairName, string publicKeyFileName = @"public.rsa", string privateKeyFileName = @"private.rsa")
        {
            string publicKeyPath = Path.Combine(directoryPath, publicKeyFileName);
            string privateKeyPath = Path.Combine(directoryPath, privateKeyFileName);

            RSAParameters? publicKey = LoadPublicKeyFromPath(publicKeyPath);
            RSAParameters? privateKey = LoadPrivateKeyFromPath(privateKeyPath);

            RSAKeyPair RKP = new RSAKeyPair(keyPairName + " Keys", null, null);

            if (publicKey.HasValue)
            {
                RKP.PublicKey = publicKey.Value;
            }
            else
            {
                if (_requirePublicKey)
                {
                    _log.Debug("Require Public RSA Key is true. Failed to find private RSA keys in: " + directoryPath);
                    return false;
                }
            }

            if (privateKey.HasValue)
            {
                RKP.PrivateKey = privateKey.Value;
            }
            else
            {
                if (_requirePrivateKey)
                {
                    _log.Debug("Require Private RSA Key is true. Failed to find private RSA keys in: " + directoryPath);
                    return false;
                }
            }

            KeyPairs.Add(RKP);
            return true;
        }

        private RSAParameters? LoadPrivateKeyFromPath(string privateKeyPath)
        {
            RSAParameters? privateKey = null;

            if (File.Exists(privateKeyPath))
            {
                try
                {
                    StreamReader fileStream = File.OpenText(privateKeyPath);
                    privateKey = ParsePrivateKeyFromStream(fileStream);
                }
                catch (Exception e)
                {
                    _log.Error("Exception caught while reading Private RSA key found at filepath: " + privateKeyPath);
                    _log.Error("Exception: " + e.Message);
                    _log.Error("Stacktrace: " + e.StackTrace);
                }
            }

            return privateKey;
        }
        private RSAParameters? LoadPublicKeyFromPath(string publicKeyPath)
        {
            RSAParameters? publicKey = null;

            if (File.Exists(publicKeyPath))
            {
                try
                {
                    StreamReader fileStream = File.OpenText(publicKeyPath);
                    publicKey = ParsePublicKeyFromStream(fileStream);
                }
                catch (Exception e)
                {
                    _log.Error("Exception caught while reading Public RSA key found at filepath: " + publicKeyPath);
                    _log.Error("Exception: " + e.Message);
                    _log.Error("Stacktrace: " + e.StackTrace);
                }
            }

            return publicKey;
        }

        private void LoadAllSubDirectoryKeys(string directory)
        {
            if (Directory.Exists(directory))
            {
                string[] subDirs = Directory.GetDirectories(directory);

                foreach(string subDirPath in subDirs)
                {
                    string subDirName = Path.GetFileName(subDirPath);
                    LoadKeyPairFromPath(subDirPath, subDirName);
                }
            }
            else
            {
                _log.Debug("Looking for RSA keys. Directory does not exist: " + directory);
            }
        }

        private RSAParameters ParsePublicKeyFromStream(StreamReader stream)
        {
            RSAParameters rsaParams = new RSAParameters();

            var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(stream);
            RsaKeyParameters KeyParameter = (RsaKeyParameters)pemReader.ReadObject();

            rsaParams.Modulus = KeyParameter.Modulus.ToByteArrayUnsigned();
            rsaParams.Exponent = KeyParameter.Exponent.ToByteArrayUnsigned();

            return rsaParams;
        }
        private RSAParameters ParsePrivateKeyFromStream(StreamReader stream)
        {
            RSAParameters rsaParams = new RSAParameters();

            var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(stream);
            AsymmetricCipherKeyPair KeyParameter = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            RsaPrivateCrtKeyParameters keyParameter = (RsaPrivateCrtKeyParameters)KeyParameter.Private; // Private contains the Public data too, neat

            // Public Key's 2 numbers
            rsaParams.Modulus = keyParameter.Modulus.ToByteArrayUnsigned();
            rsaParams.Exponent = keyParameter.PublicExponent.ToByteArrayUnsigned();

            rsaParams.D = keyParameter.Exponent.ToByteArrayUnsigned();
            rsaParams.P = keyParameter.P.ToByteArrayUnsigned();
            rsaParams.Q = keyParameter.Q.ToByteArrayUnsigned();
            rsaParams.DP = keyParameter.DP.ToByteArrayUnsigned();
            rsaParams.DQ = keyParameter.DQ.ToByteArrayUnsigned();
            rsaParams.InverseQ = keyParameter.QInv.ToByteArrayUnsigned();

            return rsaParams;
        }

        private void SelectDefaultKeyPair()
        {
            if (KeyPairs.Count > 0)
            {
                foreach(RSAKeyPair kp in KeyPairs)
                {
                    if (kp.Name.Contains("retail") || kp.Name.Contains("Retail"))
                    {
                        SelectedKeyPair = kp;
                        return;
                    }
                }

                SelectedKeyPair = KeyPairs[0];
            }
        }

        public byte[] Sign(byte[] input)
        {
            try
            {
                byte[] signature;

                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(SelectedKeyPair.PrivateKey.Value);
                    signature = RSA.SignData(input, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }

                return signature;
            }
            catch (Exception e)
            {
                _log.Info("Failed to perform RSA signature computation.");
                _log.Debug(e.Message);
                _log.Debug(e.StackTrace);
                return null;
            }
        }
        public bool Verify(byte[] originalData, byte[] signature)
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();

            try
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(SelectedKeyPair.PublicKey.Value);
                    return RSA.VerifyData(originalData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception e)
            {
                _log.Debug("Failed to decrypt a USB.");
                _log.Debug(e.Message);
                _log.Debug(e.StackTrace);
                _log.Debug($"Above exception caught while trying to decrypt a message in the {nameof(RsaService)}.");
                return false;
            }
        }
    }
}
 