﻿namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the DisabledEvent class.</summary>
    /// <remarks>
    ///     This event is posted when the printer becomes disabled. The reason for this disabled condition is passed as an
    ///     input parameter.
    /// </remarks>
    /// <seealso cref="DisabledReasons"/>
    [Serializable]
    public class DisabledEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(int printerId, DisabledReasons reasons)
            : base(printerId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reasons}");
        }
    }
}
