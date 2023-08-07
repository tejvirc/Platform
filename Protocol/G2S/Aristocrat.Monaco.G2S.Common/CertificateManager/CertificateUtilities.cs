namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using log4net;
    using Org.BouncyCastle.Asn1.Pkcs;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Operators;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.X509;
    using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

    /// <summary>
    ///     A collection of certificate utilities
    /// </summary>
    public static class CertificateUtilities
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Creates a self-signed certificate
        /// </summary>
        /// <param name="certName">The certificate name, which is also used in the common name of the generated certificate</param>
        /// <param name="keySize">The key size in bytes of the certificate</param>
        /// <param name="expireDate">The expiration date or null for a certificate that does not expire</param>
        /// <returns>a certificate</returns>
        public static X509Certificate2 CreateSelfSignedCertificate(
            string certName,
            int keySize,
            DateTime? expireDate = null)
        {
            Logger.Debug("Creating self signed certificate");

            var keypairgen = new RsaKeyPairGenerator();

            keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), keySize));

            var keyPair = keypairgen.GenerateKeyPair();

            var generator = new X509V3CertificateGenerator();

            var cn = new X509Name("CN=" + certName);
            var sn = BigInteger.ProbablePrime(120, new Random());

            generator.SetSerialNumber(sn);
            generator.SetSubjectDN(cn);
            generator.SetIssuerDN(cn);

            generator.SetNotAfter(expireDate ?? DateTime.MaxValue);

            generator.SetNotBefore(DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0)));
            generator.SetPublicKey(keyPair.Public);

            generator.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DataEncipherment | KeyUsage.KeyEncipherment));

            // Create signature factory
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            var signatureFactory = new Asn1SignatureFactory(
                PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id,
                keyPair.Private,
                random);

            var newCert = generator.Generate(signatureFactory);

            Logger.Debug($"Generated self signed certificate with serial number: {sn}");

            // return new X509Certificate2(DotNetUtilities.ToX509Certificate(newCert));
            return ExportPrivateKey(newCert, keyPair);
        }

        /// <summary>
        ///     Creates an <see cref="X509Certificate2" /> from a byte array
        /// </summary>
        /// <param name="data">The certificate data.</param>
        /// <returns>an <see cref="X509Certificate2" /></returns>
        public static X509Certificate2 CertificateFromByteArray(byte[] data)
        {
            var file = Path.Combine(Path.GetTempFileName());

            try
            {
                File.WriteAllBytes(file, data);

                return new X509Certificate2(file);
            }
            finally
            {
                File.Delete(file);
            }
        }

        /// <summary>
        ///     Creates an <see cref="X509Certificate2" /> from a byte array
        /// </summary>
        /// <param name="data">The certificate data.</param>
        /// <param name="password">The password used to access the certificate data</param>
        /// <param name="keyStorage">Additional storage key flags </param>
        /// <returns>an <see cref="X509Certificate2" /></returns>
        public static X509Certificate2 CertificateFromByteArray(
            byte[] data,
            string password,
            X509KeyStorageFlags keyStorage = X509KeyStorageFlags.DefaultKeySet)
        {
            var file = Path.Combine(Path.GetTempFileName());

            try
            {
                File.WriteAllBytes(file, data);

                return new X509Certificate2(
                    file,
                    password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | keyStorage);
            }
            finally
            {
                File.Delete(file);
            }
        }

        /// <summary>
        ///     Generates a cryptographically strong password.
        /// </summary>
        /// <returns>a password.</returns>
        public static string GeneratePassword()
        {
            var rng = new CryptoApiRandomGenerator();

            var tokenData = new byte[32];

            rng.NextBytes(tokenData);

            return Convert.ToBase64String(tokenData);
        }

        /// <summary>
        ///     Sets the private key on the certificate.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <param name="keyPair">The public private key pair</param>
        /// <returns>a <see cref="X509Certificate2" /> with a private key</returns>
        public static X509Certificate2 ExportPrivateKey(X509Certificate certificate, AsymmetricCipherKeyPair keyPair)
        {
            return ExportPrivateKey(certificate, keyPair, new X509Certificate2Collection());
        }

        /// <summary>
        ///     Sets the private key on the certificate.
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <param name="keyPair">The public private key pair</param>
        /// <param name="chain">The key chain</param>
        /// <returns>a <see cref="X509Certificate2" /> with a private key</returns>
        public static X509Certificate2 ExportPrivateKey(
            X509Certificate certificate,
            AsymmetricCipherKeyPair keyPair,
            X509Certificate2Collection chain)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            if (keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            if (chain == null)
            {
                throw new ArgumentNullException(nameof(chain));
            }

            var tempPassword = GeneratePassword();
            var tempStoreFile = new FileInfo(Path.GetTempFileName());

            try
            {
                {
                    var newStore = new Pkcs12Store();

                    var certEntry = new X509CertificateEntry(certificate);

                    var cn = certificate.SubjectDN.GetValueList(X509Name.CN);
                    var alias = cn[0].ToString();

                    newStore.SetCertificateEntry(alias, certEntry);

                    var chainedEntry = new X509CertificateEntry[chain.Count + 1];
                    chainedEntry[0] = certEntry;

                    var index = chain.Count;
                    foreach (var parent in chain)
                    {
                        chainedEntry[index] = new X509CertificateEntry(DotNetUtilities.FromX509Certificate(parent));
                        index--;
                    }

                    newStore.SetKeyEntry(
                        alias,
                        new AsymmetricKeyEntry(keyPair.Private),
                        chainedEntry);

                    using (var stream = tempStoreFile.Create())
                    {
                        newStore.Save(
                            stream,
                            tempPassword.ToCharArray(),
                            new SecureRandom(new CryptoApiRandomGenerator()));
                    }
                }

                return new X509Certificate2(
                    tempStoreFile.FullName,
                    tempPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet);
            }
            finally
            {
                tempStoreFile.Delete();
            }
        }
    }
}