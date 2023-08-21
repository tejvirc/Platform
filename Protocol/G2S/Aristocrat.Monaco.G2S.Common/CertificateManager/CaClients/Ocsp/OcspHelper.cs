namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Ocsp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Org.BouncyCastle.Asn1;
    using Org.BouncyCastle.Asn1.Ocsp;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Ocsp;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.Utilities;
    using Org.BouncyCastle.X509;
    using Org.BouncyCastle.X509.Extension;
    using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;
    using X509Extension = Org.BouncyCastle.Asn1.X509.X509Extension;

    /// <summary>
    ///     Utilities for OCSP protocol.
    /// </summary>
    public static class OcspHelper
    {
        private const string OcspRequestContentType = "application/ocsp-request";

        private const string OcspResponseContentType = "application/ocsp-response";

        /// <summary>
        ///     Gets certificate authority from server.
        /// </summary>
        /// <param name="certificateAuthorityServerUrl">Certificate authority administration URL.</param>
        /// <returns>Returns certificate authority from server.</returns>
        public static X509Certificate2Collection GetCertificateAuthorityFromServer(string certificateAuthorityServerUrl)
        {
            var webClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(certificateAuthorityServerUrl) };
            var caCertData = webClient.GetByteArrayAsync("?operation=GetCACert&message=1").GetAwaiter().GetResult();

            var caCertChain = new X509Certificate2Collection();
            caCertChain.Import(caCertData);

            webClient.Dispose();
            return caCertChain;
        }

        /// <summary>
        ///     Generates the ocsp request.
        /// </summary>
        /// <param name="issuerCertificate">The issuer certificate.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="nonce">An optional nonce.</param>
        /// <returns>An OcspReq object.</returns>
        public static OcspReq GenerateOcspRequest(
            X509Certificate issuerCertificate,
            X509Certificate certificate,
            byte[] nonce)
        {
            var issuerCertificateConverted = DotNetUtilities.FromX509Certificate(issuerCertificate);
            var certificateConverted = DotNetUtilities.FromX509Certificate(certificate);

            var id = new CertificateID(
                CertificateID.HashSha1,
                issuerCertificateConverted,
                certificateConverted.SerialNumber);
            var generator = new OcspReqGenerator();

            generator.AddRequest(id);

            //// TODO: Could use signing but together with RequestorName only.
            //// ocspRequestGenerator.SetRequestorName(new X509Name(certificate.Subject));

            if (nonce != null)
            {
                var values = new Dictionary<DerObjectIdentifier, X509Extension>();

                var extension = new X509Extension(
                    false,
                    new DerOctetString(new DerOctetString(nonce).GetEncoded()));
                values.Add(OcspObjectIdentifiers.PkixOcspNonce, extension);
                generator.SetRequestExtensions(new X509Extensions(values));
            }

            // TODO: Review if signing is required
            // var keyPair = DotNetUtilities.GetKeyPair(new RSACryptoServiceProvider());
            // return ocspRequestGenerator.Generate("SHA1withRSA", keyPair.Private, new [] { certificateConverted });
            return generator.Generate();
        }

        /// <summary>
        ///     Submits request to certificate authority server.
        /// </summary>
        /// <param name="certificateAuthorityServerUrl">Certificate authority administration URL.</param>
        /// <param name="requestData">Encrypted and signed PKCS7 envelope.</param>
        /// <returns>Returns result from certificate authority server.</returns>
        public static byte[] SubmitRequestToOcsp(string certificateAuthorityServerUrl, byte[] requestData)
        {
            byte[] result;

            var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(OcspRequestContentType));
            var c = new ByteArrayContent(requestData);
            var response = client.PostAsync(certificateAuthorityServerUrl, c).GetAwaiter().GetResult();
            using (var streamResponse = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
            {
                result = ReadFully(streamResponse, 0);
            }

            return result;
        }

        /// <summary>
        ///     Reads data from a stream until the end is reached. The
        ///     data is returned as a byte array. An IOException is
        ///     thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        /// <returns>The complete byte array read</returns>
        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768; // Greater than the max OCSP response size of 20480
            }

            var buffer = new byte[initialLength];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    var nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        /// <summary>
        ///     Process response of OCSP protocol
        /// </summary>
        /// <param name="issuerCertificate">The issuer certificate.</param>
        /// <param name="certificate">The certificate</param>
        /// <param name="response">The response</param>
        /// <param name="nonce">An optional nonce.</param>
        /// <returns>An OcspCertificateStatus object.</returns>
        public static OcspCertificateStatus ProcessOcspResponse(
            X509Certificate issuerCertificate,
            X509Certificate certificate,
            byte[] response,
            byte[] nonce)
        {
            var issuerCertificateConverted = DotNetUtilities.FromX509Certificate(issuerCertificate);
            var certificateConverted = DotNetUtilities.FromX509Certificate(certificate);

            var ocspResponse = new OcspResp(response);

            var currentStatus = OcspCertificateStatus.Unknown;

            switch (ocspResponse.Status)
            {
                case OcspRespStatus.Successful:
                    var responseObject = (BasicOcspResp)ocspResponse.GetResponseObject();

                    ValidateNonce(responseObject, nonce);

                    if (responseObject.Responses.Length == 1)
                    {
                        var resp = responseObject.Responses[0];

                        ValidateCertificateId(issuerCertificateConverted, certificateConverted, resp.GetCertID());

                        var certificateStatus = resp.GetCertStatus();
                        if (certificateStatus == CertificateStatus.Good)
                        {
                            currentStatus = OcspCertificateStatus.Good;
                        }
                        else if (certificateStatus is RevokedStatus)
                        {
                            currentStatus = OcspCertificateStatus.Revoked;
                        }
                        else if (certificateStatus is UnknownStatus)
                        {
                            currentStatus = OcspCertificateStatus.Unknown;
                        }
                    }

                    break;
                default:
                    throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UnknowOcspStatusErrorMessageTemplate));
            }

            return currentStatus;
        }

        private static void ValidateCertificateId(
            Org.BouncyCastle.X509.X509Certificate issuerCert,
            Org.BouncyCastle.X509.X509Certificate eeCert,
            CertificateID certificateId)
        {
            var expectedId = new CertificateID(CertificateID.HashSha1, issuerCert, eeCert.SerialNumber);

            if (!expectedId.SerialNumber.Equals(certificateId.SerialNumber))
            {
                throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidCertificateIDErrorMessage));
            }

            if (!Arrays.AreEqual(expectedId.GetIssuerNameHash(), certificateId.GetIssuerNameHash()))
            {
                throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidCertificateIssuerErrorMessage));
            }
        }

        private static void ValidateNonce(IX509Extension response, byte[] nonce)
        {
            if (nonce == null)
            {
                return;
            }

            var extensions = response.GetNonCriticalExtensionOids();
            if (extensions.Count == 0)
            {
                throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MissingNonceInResponse));
            }

            var extValue = response.GetExtensionValue(OcspObjectIdentifiers.PkixOcspNonce);
            if (extValue == null)
            {
                throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MissingNonceInResponse));
            }

            var extObj = X509ExtensionUtilities.FromExtensionValue(extValue);
            if (!Arrays.AreEqual(((Asn1OctetString)extObj).GetOctets(), nonce))
            {
                throw new OcspException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NonceMismatch));
            }
        }
    }
}