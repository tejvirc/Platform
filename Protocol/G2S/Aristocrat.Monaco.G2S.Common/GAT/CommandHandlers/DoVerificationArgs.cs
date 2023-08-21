namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Do verification arguments
    /// </summary>
    public class DoVerificationArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoVerificationArgs" /> class.
        /// </summary>
        /// <param name="verificationId">The verification identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="verifyComponents">The verify components.</param>
        /// <param name="verificationCallback">The verification callback.</param>
        /// <param name="queueVerify">Queue verify callback.</param>
        /// <param name="startVerify">The start verify.</param>
        public DoVerificationArgs(
            long verificationId,
            int deviceId,
            string employeeId,
            IEnumerable<VerifyComponent> verifyComponents,
            Action<DoVerificationArgs> verificationCallback = null,
            Action<long> queueVerify = null,
            Action<long> startVerify = null)
        {
            if (verifyComponents == null)
            {
                throw new ArgumentNullException(nameof(verifyComponents));
            }

            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(verificationId));
            }

            if (!verifyComponents.Any())
            {
                throw new ArgumentException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IvalidVerifyComponentsErrorMessage));
            }

            VerificationId = verificationId;
            VerifyComponents = verifyComponents;
            DeviceId = deviceId;
            EmployeeId = employeeId;
            VerificationCallback = verificationCallback;
            StartVerifyCallback = startVerify;
            QueueVerifyCallback = queueVerify;
        }

        /// <summary>
        ///     Gets verification identifier
        /// </summary>
        public long VerificationId { get; }

        /// <summary>
        ///     Gets device identifier of the device that generated the transaction
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        ///     Gets identifier of the employee present at the EGM when the function was executed
        /// </summary>
        public string EmployeeId { get; }

        /// <summary>
        ///     Gets verify components list
        /// </summary>
        public IEnumerable<VerifyComponent> VerifyComponents { get; }

        /// <summary>
        ///     Gets verification callback
        /// </summary>
        public Action<DoVerificationArgs> VerificationCallback { get; }

        /// <summary>
        ///     Gets QueueVerify callback
        /// </summary>
        public Action<long> QueueVerifyCallback { get; }

        /// <summary>
        ///     Gets StartVerify callback
        /// </summary>
        public Action<long> StartVerifyCallback { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the transactionId.
        /// </summary>
        public long TransactionId { get; set; }
    }
}