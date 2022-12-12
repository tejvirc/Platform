namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;
    using Kernel.MessageDisplay;
    using Localization.Properties;

    /// <summary>The states the transfer provider can be in</summary>
    internal enum TransferOffState
    {
        None,

        InitiatedWaiting,

        Initiated,

        RequestUpdated,

        RequestedOff,

        Canceling
    }

    /// <summary>The different reasons for disallowing transfers</summary>
    internal enum DisallowAftOffReason
    {
        None,

        HardHostCashOutModeLockup
    }

    /// <summary>
    ///     Constants for AFTs
    /// </summary>
    public static class AftConstants
    {
        /// <summary>The key to access the currency multiplier property.</summary>
        public const string CurrencyMultiplierKey = ApplicationConstants.CurrencyMultiplierKey;

        /// <summary>The default amount for the voucher limit. $10,000 in millicent.</summary>
        public const long DefaultVoucherLimit = 1000000000L;

        /// <summary>The name for the maximum voucher limit property.</summary>
        public const string MaximumVoucherLimitPropertyName = "System.MaxVoucherLimit";

        /// <summary>The message to display when the system goes into an Aft lock.</summary>
        public static readonly IDisplayableMessage LockMessage = new DisplayableMessage(
            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftLockMessage),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                new Guid("{90639592-3E1E-4EEF-8201-1A98A005C444}"));

        /// <summary>
        ///     The disable list that allows for AFT off and AFT off disables to be accepted
        /// </summary>
        public static readonly IReadOnlyList<Guid> AllowedAftOffDisables = new List<Guid>
        {
            ApplicationConstants.LiveAuthenticationDisableKey
        };
    }
}
