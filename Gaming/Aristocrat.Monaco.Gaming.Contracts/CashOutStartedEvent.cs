namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The CashOutStartedEvent is posted just before initiating a cash out.  This will occur after the cashout button was
    ///     pressed or at the end of a game round when forcing a cash out.
    /// </summary>
    [ProtoContract]
    public class CashOutStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutStartedEvent" /> class.
        /// </summary>
        /// <param name="forcedByMaxBank">Indicates if the cashout was forced by the system for hitting a max bank value.</param>
        /// <param name="zeroRemaining">Indicates if the cashout leaves nothing in the bank.</param>
        public CashOutStartedEvent(bool forcedByMaxBank, bool zeroRemaining)
        {
            ForcedByMaxBank = forcedByMaxBank;
            ZeroRemaining = zeroRemaining;
        }

        /// <summary>
        ///     Gets the ForcedByMaxBank parameter
        /// </summary>
        [ProtoMember(1)]
        public bool ForcedByMaxBank { get; }

        /// <summary>
        ///     Gets the ZeroRemaining parameter
        /// </summary>
        [ProtoMember(2)]
        public bool ZeroRemaining { get; }
    }
}