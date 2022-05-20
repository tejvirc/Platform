namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Save verification-ack argument
    /// </summary>
    public class SaveVerificationAckArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveVerificationAckArgs" /> class.
        /// </summary>
        /// <param name="verificationId">Verification identifier</param>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="componentVerifications">Component verifications list</param>
        public SaveVerificationAckArgs(
            long verificationId,
            long transactionId,
            IEnumerable<ComponentVerification> componentVerifications)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(verificationId),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidVerificationIdErrorMessage));
            }

            if (transactionId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transactionId),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidTransactionIdErrorMessage));
            }

            if (componentVerifications == null)
            {
                throw new ArgumentNullException(nameof(componentVerifications));
            }

            if (!componentVerifications.Any())
            {
                throw new ArgumentException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyComponentVerificationsErrorMessage));
            }

            VerificationId = verificationId;
            TransactionId = transactionId;
            ComponentVerifications = componentVerifications;
        }

        /// <summary>
        ///     Gets verification identifier
        /// </summary>
        public long VerificationId { get; }

        /// <summary>
        ///     Gets transaction identifier
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     Gets component verifications list
        /// </summary>
        public IEnumerable<ComponentVerification> ComponentVerifications { get; }
    }
}