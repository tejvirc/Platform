﻿namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     Definition of the SessionInfoEvent class.
    /// </summary>
    /// <remarks>
    ///     An event of this type is posted when a Session is started (e.g. BillTransaction or VoucherInTransaction happened),
    ///     or ended (e.g. VoucherOutCompletedEvent was detected or a game reached Idle state with zero credits in bank)
    /// </remarks>
    [Serializable]
    public class SessionInfoEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionInfoEvent" /> class
        /// </summary>
        public SessionInfoEvent(double sessionInfoValue)
        {
            SessionInfoValue = sessionInfoValue;
        }

        /// <summary>
        ///     Gets the session information 
        /// </summary>
        public double SessionInfoValue { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [SessionInfoValue={1}",
                GetType().Name,
                SessionInfoValue);
        }
    }
}