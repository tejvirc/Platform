namespace Aristocrat.Monaco.Bingo.Tests.Services.Security
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Org.BouncyCastle.Asn1.Pkcs;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Operators;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.X509;

    public class CertificateHelper
    {
        public static X509Certificate2 GenerateCertificate(string certName, DateTime? expireDate = null)
        {
            var keypairgen = new RsaKeyPairGenerator();
            keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 1024));

            var keypair = keypairgen.GenerateKeyPair();

            var generator = new X509V3CertificateGenerator();

            var cn = new X509Name("CN=" + certName);
            var sn = BigInteger.ProbablePrime(120, new Random());

            generator.SetSerialNumber(sn);
            generator.SetSubjectDN(cn);
            generator.SetIssuerDN(cn);

            if (expireDate.HasValue)
            {
                generator.SetNotAfter(expireDate.Value);
            }
            else
            {
                generator.SetNotAfter(DateTime.MaxValue);
            }

            generator.SetNotBefore(DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0)));
            generator.SetPublicKey(keypair.Public);

            // Create signature factory.
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(
                PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id,
                keypair.Private,
                random);

            var newCert = generator.Generate(signatureFactory);

            return new X509Certificate2(DotNetUtilities.ToX509Certificate(newCert));
        }
    }
}