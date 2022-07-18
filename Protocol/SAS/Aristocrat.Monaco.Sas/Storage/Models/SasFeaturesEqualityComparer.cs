namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Collections.Generic;
    using Contracts.SASProperties;

    /// <summary>
    ///     An equality comparer for <see cref="SasFeatures"/>
    /// </summary>
    public sealed class SasFeaturesEqualityComparer : IEqualityComparer<SasFeatures>
    {
        /// <inheritdoc />
        public bool Equals(SasFeatures x, SasFeatures y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.HandpayReportingType == y.HandpayReportingType &&
                   x.TransferLimit == y.TransferLimit &&
                   x.MaxAllowedTransferLimits == y.MaxAllowedTransferLimits &&
                   x.PartialTransferAllowed == y.PartialTransferAllowed &&
                   x.TransferInAllowed == y.TransferInAllowed &&
                   x.DebitTransfersAllowed == y.DebitTransfersAllowed &&
                   x.TransferToTicketAllowed == y.TransferToTicketAllowed &&
                   x.TransferOutAllowed == y.TransferOutAllowed &&
                   x.WinTransferAllowed == y.WinTransferAllowed &&
                   x.AftBonusAllowed == y.AftBonusAllowed &&
                   x.LegacyBonusAllowed == y.LegacyBonusAllowed &&
                   x.ValidationType == y.ValidationType &&
                   x.OverflowBehavior == y.OverflowBehavior &&
                   x.ConfigNotification == y.ConfigNotification &&
                   x.DisableOnDisconnect == y.DisableOnDisconnect &&
                   x.DisabledOnPowerUp == y.DisabledOnPowerUp &&
                   x.DisableOnDisconnectConfigurable == y.DisableOnDisconnectConfigurable &&
                   x.NonSasProgressiveHitReporting == y.NonSasProgressiveHitReporting &&
                   x.GeneralControlEditable == y.GeneralControlEditable &&
                   x.AddressConfigurableOnlyOnce == y.AddressConfigurableOnlyOnce &&
                   x.BonusTransferStatusEditable == y.BonusTransferStatusEditable &&
                   x.ProgressiveGroupId == y.ProgressiveGroupId &&
                   x.FundTransferType == y.FundTransferType;
        }

        /// <inheritdoc />
        public int GetHashCode(SasFeatures obj)
        {
            unchecked
            {
                var hashCode = (int)obj.HandpayReportingType;
                hashCode = (hashCode * 397) ^ obj.TransferLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.MaxAllowedTransferLimits.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.PartialTransferAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.TransferInAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.DebitTransfersAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.TransferToTicketAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.TransferOutAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.WinTransferAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.AftBonusAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.LegacyBonusAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)obj.ValidationType;
                hashCode = (hashCode * 397) ^ (int)obj.OverflowBehavior;
                hashCode = (hashCode * 397) ^ (int)obj.ConfigNotification;
                hashCode = (hashCode * 397) ^ obj.DisableOnDisconnect.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.DisabledOnPowerUp.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.DisableOnDisconnectConfigurable.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.NonSasProgressiveHitReporting.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.GeneralControlEditable.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.AddressConfigurableOnlyOnce.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.BonusTransferStatusEditable.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.ProgressiveGroupId;
                hashCode = (hashCode * 397) ^ (int)obj.FundTransferType;
                return hashCode;
            }
        }
    }
}