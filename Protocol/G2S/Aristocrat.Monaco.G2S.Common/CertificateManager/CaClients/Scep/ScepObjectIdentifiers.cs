namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Scep
{
    using Org.BouncyCastle.Asn1;

    /// <summary>
    ///     http://www.cisco.com/c/en/us/support/docs/security-vpn/public-key-infrastructure-pki/116167-technote-scep-00.html
    /// </summary>
    public abstract class ScepObjectIdentifiers
    {
        /// <summary>
        ///     2.16.840.1.113733.1.9.2 scep-messageType - https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.2
        /// </summary>
        public static readonly DerObjectIdentifier MessageType = new DerObjectIdentifier("2.16.840.1.113733.1.9.2");

        /// <summary>
        ///     2.16.840.1.113733.1.9.5 scep-senderNonce - https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.5
        /// </summary>
        public static readonly DerObjectIdentifier SenderNonce = new DerObjectIdentifier("2.16.840.1.113733.1.9.5");

        /// <summary>
        ///     2.16.840.1.113733.1.9.7 scep-transId - https://tools.ietf.org/html/draft-nourse-scep-23#section-3.1.1.1
        /// </summary>
        public static readonly DerObjectIdentifier TransactionId = new DerObjectIdentifier("2.16.840.1.113733.1.9.7");

        /// <summary>
        ///     2.16.840.1.113733.1.9.6 scep-recipientNonce - Response only
        /// </summary>
        public static readonly DerObjectIdentifier RecipientNonce = new DerObjectIdentifier("2.16.840.1.113733.1.9.6");

        /// <summary>
        ///     2.16.840.1.113733.1.9.3 scep-pkiStatus - Response only
        /// </summary>
        public static readonly DerObjectIdentifier PkiStatus = new DerObjectIdentifier("2.16.840.1.113733.1.9.3");

        /// <summary>
        ///     2.16.840.1.113733.1.9.4 scep-failInfo - Failure only
        /// </summary>
        public static readonly DerObjectIdentifier FailInfo = new DerObjectIdentifier("2.16.840.1.113733.1.9.4");

        /// <summary>
        ///     2.16.840.1.113733.1.9.8 scep-extensionReq
        /// </summary>
        public static readonly DerObjectIdentifier ExtensionReq = new DerObjectIdentifier("2.16.840.1.113733.1.9.8");
    }
}