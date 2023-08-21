namespace Aristocrat.Monaco.G2S.UI.Models
{
    using System;
    using Common.CertificateManager.Models;
    using System.Collections.ObjectModel;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     certificate info model
    /// </summary>
    public class CertificateInfo : IEquatable<CertificateInfo>
    {
        /// <summary>
        ///     Gets or sets the certificate
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        ///     Gets or sets the common name (CN)
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        ///     Gets or sets the Thumb print
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        ///     Gets or sets the user name
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        ///     Gets or sets the not before
        /// </summary>
        public string NotBefore { get; set; }

        /// <summary>
        ///     Gets or sets the Not After
        /// </summary>
        public string NotAfter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating OCSP off line date
        /// </summary>
        public string OcspOfflineDate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating verification date
        /// </summary>
        public string VerificationDate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating certificate status
        /// </summary>
        public CertificateStatus InternalStatus { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether certificate is verified
        /// </summary>
        public bool HasPrivateKey { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether certificate is default
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether certificate is expired
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating certificate status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Gets certificates signed by this certificate
        /// </summary>
        public ObservableCollection<CertificateInfo> Certificates { get; } = new ObservableCollection<CertificateInfo>();

        /// <summary>
        ///     Determines if two data items are equal.
        /// </summary>
        /// <param name="other">Right-hand-side of equals.</param>
        /// <returns>True if equal.</returns>
        public bool Equals(CertificateInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return
                CommonName == other.CommonName &&
                Thumbprint == other.Thumbprint &&
                SerialNumber == other.SerialNumber &&
                NotBefore == other.NotBefore &&
                NotAfter == other.NotAfter &&
                OcspOfflineDate == other.OcspOfflineDate &&
                VerificationDate == other.VerificationDate &&
                Status == other.Status &&
                (HasPrivateKey = other.HasPrivateKey) &&
                (IsExpired = other.IsExpired) &&
                (IsDefault = other.IsDefault);
        }

        /// <summary>
        ///     Object based Equals.
        /// </summary>
        /// <param name="obj">Right-hand-side of equals.</param>
        /// <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var info = obj as CertificateInfo;
            return Equals(info);
        }

        /// <summary>
        ///     GetHashCode
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + CommonName.GetHashCode();
                hash = hash * 23 + Thumbprint.GetHashCode();
                hash = hash * 23 + SerialNumber.GetHashCode();
                hash = hash * 23 + NotBefore.GetHashCode();
                hash = hash * 23 + NotAfter.GetHashCode();
                hash = hash * 23 + OcspOfflineDate.GetHashCode();
                hash = hash * 23 + VerificationDate.GetHashCode();
                hash = hash * 23 + Status.GetHashCode();
                hash = hash * 23 + HasPrivateKey.GetHashCode();
                hash = hash * 23 + IsDefault.GetHashCode();
                hash = hash * 23 + IsExpired.GetHashCode();

                return hash;
            }
        }
    }
}