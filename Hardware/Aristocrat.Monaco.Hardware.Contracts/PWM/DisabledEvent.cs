﻿namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the DisabledEvent class.</summary>
    /// <remarks>
    ///     This event is posted when the coin acceptor becomes disabled. The reason for this disabled condition is passed as an
    ///     input parameter.
    /// </remarks>
    /// <seealso cref="DisabledReasons"/>
    [Serializable]
    public class DisabledEvent : CoinAcceptorBaseEvent
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
        /// <param name="coinAcceptorId">The associated coin acceptor's ID.</param>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(int coinAcceptorId, DisabledReasons reasons)
            : base(coinAcceptorId)
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
