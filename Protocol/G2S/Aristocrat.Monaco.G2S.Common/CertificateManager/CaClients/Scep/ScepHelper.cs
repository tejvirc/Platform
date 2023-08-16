namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Scep
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using Application.Contracts.Localization;
    using log4net;
    using Localization.Properties;
    using Org.BouncyCastle.Asn1;
    using Org.BouncyCastle.Asn1.Pkcs;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Operators;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.Security;
    using ContentInfo = System.Security.Cryptography.Pkcs.ContentInfo;
    using SignerInfo = System.Security.Cryptography.Pkcs.SignerInfo;
    using X509Extension = System.Security.Cryptography.X509Certificates.X509Extension;
    using System.Net.Http;

    /// <summary>
    ///     Utilities for SCEP protocol.
    /// </summary>
    public static class ScepHelper
    {
        private const string ContainerName = "SCEPKeyContainer";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Generate PKCS10 certificate request message encode.
        /// </summary>
        /// <param name="keySize">The size of the key to use in bits.</param>
        /// <param name="subject">Certificate subject.</param>
        /// <param name="challengePassword">Challenge password.</param>
        /// <returns>Returns PKCS10 certificate request message encoded.</returns>
        public static string GeneratePkcs10CertificationRequest(int keySize, X509Name subject, string challengePassword)
        {
            Pkcs10CertificationRequest request;

            using (var rsa = new RSACryptoServiceProvider(
                keySize,
                new CspParameters { KeyContainerName = ContainerName }))
            {
                var keyPair = DotNetUtilities.GetKeyPair(rsa);

                var attributes = GetPkcs10Attributes(challengePassword);

                // Create signature factory
                var randomGenerator = new CryptoApiRandomGenerator();
                var random = new SecureRandom(randomGenerator);

                var signatureFactory = new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id, keyPair.Private, random);
                request = new Pkcs10CertificationRequest(
                    signatureFactory,
                    subject,
                    keyPair.Public,
                    attributes);
            }

            return Convert.ToBase64String(request.GetEncoded());
        }

        /// <summary>
        ///     Gets certificate authority from server.
        /// </summary>
        /// <param name="httpClient">HTTP client instance.</param>
        /// <param name="scepCaIdent">CA-IDENT parameter.</param>
        /// <returns>Returns certificate authority from server.</returns>
        public static X509Certificate2Collection GetCertificateAuthorityFromServer(
            HttpClient httpClient,
            string scepCaIdent = null)
        {
            var url = "?operation=GetCACert";

            if (string.IsNullOrEmpty(scepCaIdent) == false)
            {
                url += "&message=" + System.Web.HttpUtility.UrlEncode(scepCaIdent);
            }
            else
            {
                url += "&message=ignored";
            }

            try
            {
                var caCertData = httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();

                var caCertChain = new X509Certificate2Collection();
                caCertChain.Import(caCertData);

                return caCertChain;
            }
            catch (Exception ex)
            {
                Logger.Error("GetCertificateAuthorityFromServer : " + ex.Message + " at url '" + httpClient.BaseAddress + "' to download data.");
                return null;
            }
        }

        /// <summary>
        ///     Creates PKCS7 envelope that contains PKCS10 request message encoded.
        /// </summary>
        /// <param name="certificateRequestMessage">PKCS10 certificate request encoded.</param>
        /// <param name="certificateAuthorityChain">List of certificates from certificate authority server.</param>
        /// <returns>Returns PKCS7 envelope that contains PKCS10 certificate request message encoded.</returns>
        public static byte[] GetPkcs7Envelope(
            string certificateRequestMessage,
            X509Certificate2Collection certificateAuthorityChain)
        {
            var recipientCert = GetRecipientCertificateFromChain(certificateAuthorityChain);
            var recipient = new CmsRecipient(recipientCert);

            var envelopedContent = new ContentInfo(
                new Oid(PkcsObjectIdentifiers.EncryptedData.Id, "envelopedData"),
                Convert.FromBase64String(certificateRequestMessage));

            var envelopedMessage = new EnvelopedCms(envelopedContent);
            envelopedMessage.Encrypt(recipient);

            return envelopedMessage.Encode();
        }

        /// <summary>
        ///     Gets recipient certificate from CA chain.
        /// </summary>
        /// <param name="certificateAuthorityChain">CA chain.</param>
        /// <returns>Returns recipient certificate.</returns>
        public static X509Certificate2 GetRecipientCertificateFromChain(
            X509Certificate2Collection certificateAuthorityChain)
        {
            X509Certificate2 found = null;

            foreach (var certificate in certificateAuthorityChain)
            {
                var foundExt =
                    certificate.Extensions.Cast<X509Extension>().FirstOrDefault(ext => ext is X509KeyUsageExtension);
                if (foundExt != null &&
                    ((X509KeyUsageExtension)foundExt).KeyUsages == X509KeyUsageFlags.KeyEncipherment)
                {
                    if (found != null)
                    {
                        throw new ApplicationException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidCAorRAConfigurationErrorMessage));
                    }

                    found = certificate;
                }
            }

            return found;
        }

        /// <summary>
        ///     Gets recipient certificate from CA chain.
        /// </summary>
        /// <param name="certificateAuthorityChain">CA chain.</param>
        /// <returns>Returns recipient certificate.</returns>
        public static X509Certificate2 GetRootCertificateFromChain(X509Certificate2Collection certificateAuthorityChain)
        {
            foreach (var certificate in certificateAuthorityChain)
            {
                if (certificate.Issuer.Equals(certificate.Subject, StringComparison.InvariantCultureIgnoreCase))
                {
                    return certificate;
                }

                var foundExt = certificate.Extensions.Cast<X509Extension>()
                    .FirstOrDefault(ext => ext is X509KeyUsageExtension);
                if (foundExt != null &&
                    ((X509KeyUsageExtension)foundExt).KeyUsages.HasFlag(X509KeyUsageFlags.KeyCertSign))
                {
                    return certificate;
                }
            }

            return null;
        }

        /// <summary>
        ///     Sings PKCS7 envelope with specified signing certificate.
        /// </summary>
        /// <param name="encryptedMessageEnvelope">Encrypted message of PKCS7 envelope.</param>
        /// <param name="signingCertificate">Instance of certificate to sign PKCS7 envelope.</param>
        /// <returns>Returns signed PKCS7 envelope.</returns>
        public static byte[] GetSignedPkcs7Envelope(
            byte[] encryptedMessageEnvelope,
            X509Certificate2 signingCertificate)
        {
            // Create the outer envelope, signed with the local private key
            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signingCertificate)
            {
                DigestAlgorithm = new Oid(PkcsObjectIdentifiers.MD5.Id, "digestAlgorithm"),
                IncludeOption = X509IncludeOption.WholeChain
            };

            // Message Type (messageType): https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.2
            // PKCS#10 request = PKCSReq (19)
            signer.SignedAttributes.Add(
                new AsnEncodedData(ScepObjectIdentifiers.MessageType.Id, new DerPrintableString("19").GetEncoded()));

            // Transaction ID (transId): https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.1
            using (var sha = SHA512.Create())
            {
                var hashedKey = sha.ComputeHash(signingCertificate.GetPublicKey());
                var hashedKeyString = Convert.ToBase64String(hashedKey);
                signer.SignedAttributes.Add(
                    new Pkcs9AttributeObject(
                        ScepObjectIdentifiers.TransactionId.Id,
                        new DerPrintableString(hashedKeyString).GetEncoded()));
            }

            // Sender Nonce (senderNonce): https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.5
            using (var rng = RandomNumberGenerator.Create())
            {
                var nonceBytes = new byte[16];
                rng.GetBytes(nonceBytes);
                signer.SignedAttributes.Add(
                    new Pkcs9AttributeObject(
                        ScepObjectIdentifiers.SenderNonce.Id,
                        new DerOctetString(nonceBytes).GetEncoded()));
            }

            // oid does not appear to be used in the envelop
            var signedContent = new ContentInfo(new Oid("1.2.840.113549.1.7.1", "data"), encryptedMessageEnvelope);
            var signedMessage = new SignedCms(signedContent);
            signedMessage.ComputeSignature(signer);

            return signedMessage.Encode();
        }

        /// <summary>
        ///     Submits request to certificate authority server.
        /// </summary>
        /// <param name="client">Web client instance.</param>
        /// <param name="encryptedAndSignedMessageEnvelope">Encrypted and signed PKCS7 envelope.</param>
        /// <returns>Returns result from certificate authority server.</returns>
        public static byte[] SubmitRequestToScep(HttpClient client, byte[] encryptedAndSignedMessageEnvelope)
        {
            var message = Convert.ToBase64String(encryptedAndSignedMessageEnvelope);
            var urlEncodedMessage = Uri.EscapeDataString(message);

            return client.GetByteArrayAsync("?operation=PKIOperation&message=" + urlEncodedMessage).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Parse results from certificate authority server.
        /// </summary>
        /// <param name="certificateAuthorityChain">List of certificates from certificate authority server.</param>
        /// <param name="response">Certificate authority response.</param>
        /// <param name="request">Request data.</param>
        /// <param name="signingCertificate">Certificate that been used to sign PKCS10 request.</param>
        /// <returns>Returns result from certificate authority server.</returns>
        public static CertificateActionResult ParseScepResponse(
            X509Certificate2Collection certificateAuthorityChain,
            byte[] response,
            byte[] request,
            X509Certificate2 signingCertificate)
        {
            var signedResponse = new SignedCms();
            signedResponse.Decode(response);

            signedResponse.CheckSignature(certificateAuthorityChain, true);

            var attributes = signedResponse
                .SignerInfos
                .Cast<SignerInfo>()
                .SelectMany(si => si.SignedAttributes.Cast<CryptographicAttributeObject>());

            var cryptographicAttributeObjects = attributes as CryptographicAttributeObject[] ?? attributes.ToArray();

            var statusValue = Asn1Object.FromByteArray(
                cryptographicAttributeObjects
                    .Single(
                        att =>
                            att.Oid.Value.Equals(
                                ScepObjectIdentifiers.PkiStatus.Id,
                                StringComparison.InvariantCultureIgnoreCase))
                    .Values[0].RawData).ToString();

            var status = PkiStatus.ParseError;
            if (int.TryParse(statusValue, out var statusCode))
            {
                status = (PkiStatus)statusCode;
            }

            var failInfos =
                cryptographicAttributeObjects.Where(
                    att =>
                        att.Oid.Value.Equals(
                            ScepObjectIdentifiers.FailInfo.Id,
                            StringComparison.InvariantCultureIgnoreCase)).ToArray();
            var failInfoCode = string.Empty;
            if (failInfos.Any())
            {
                foreach (var cryptographicAttributeObject in failInfos)
                {
                    foreach (var value in cryptographicAttributeObject.Values)
                    {
                        failInfoCode = Asn1Object.FromByteArray(value.RawData).ToString();
                        break;
                    }
                }
            }

            CertificateActionResult result;
            if (status == PkiStatus.Success)
            {
                var envelopedCmsResponse = new EnvelopedCms();
                envelopedCmsResponse.Decode(signedResponse.ContentInfo.Content);

                var decryptCertificatesCollection = new X509Certificate2Collection { signingCertificate };

                envelopedCmsResponse.Decrypt(decryptCertificatesCollection);

                if (signedResponse.Certificates.Count > 0)
                {
                    result = new CertificateActionResult(
                        signedResponse.Certificates[0],
                        CertificateRequestStatus.Enrolled);
                }
                else
                {
                    var resultCertificatesCollection = new X509Certificate2Collection();
                    resultCertificatesCollection.Import(envelopedCmsResponse.ContentInfo.Content);

                    using (var rsa =
                        new RSACryptoServiceProvider(new CspParameters { KeyContainerName = ContainerName }))
                    {
                        var keyPair = DotNetUtilities.GetKeyPair(rsa);

                        var certificate =
                            CertificateUtilities.ExportPrivateKey(
                                DotNetUtilities.FromX509Certificate(resultCertificatesCollection[0]),
                                keyPair,
                                certificateAuthorityChain);

                        result = new CertificateActionResult(certificate, CertificateRequestStatus.Enrolled);

                        rsa.PersistKeyInCsp = false;
                        rsa.Clear();
                    }
                }
            }
            else if (status == PkiStatus.Pending)
            {
                result = new CertificateActionResult(request, signingCertificate, CertificateRequestStatus.Pending);
            }
            else
            {
                // https://tools.ietf.org/html/draft-gutmann-scep-01#section-3.1.1.4
                if (!string.IsNullOrEmpty(failInfoCode) && failInfoCode.Equals("2"))
                {
                    result = new CertificateActionResult(CertificateRequestStatus.Denied);
                }
                else
                {
                    result = new CertificateActionResult(CertificateRequestStatus.Error);
                }
            }

            return result;
        }

        private static DerSet GetPkcs10Attributes(string challengePassword)
        {
            var vector = new Asn1EncodableVector();

            if (!string.IsNullOrEmpty(challengePassword))
            {
                var challengePasswordVector = new Asn1EncodableVector
                {
                    new DerObjectIdentifier(PkcsObjectIdentifiers.Pkcs9AtChallengePassword.Id)
                };

                // challenge password attribute
                var challengePasswordValues = new Asn1EncodableVector { new DerPrintableString(challengePassword) };
                challengePasswordVector.Add(new DerSet(challengePasswordValues));

                vector.Add(new DerSequence(challengePasswordVector));
            }

            // extension attribute
            var extensionsVector = new Asn1EncodableVector
            {
                new DerObjectIdentifier(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest.Id)
            };

            // Set the key usage scope.
            var extensionsGenerator = new X509ExtensionsGenerator();
            extensionsGenerator.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
            extensionsVector.Add(new DerSet(extensionsGenerator.Generate()));

            vector.Add(new DerSequence(extensionsVector));

            return new DerSet(vector);
        }
    }
}