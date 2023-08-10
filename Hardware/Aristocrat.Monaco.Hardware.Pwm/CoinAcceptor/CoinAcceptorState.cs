namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using System;
    using Contracts.CoinAcceptor;

    /// <summary>
    ///     Class to maintain the coin acceptor state.
    ///     State tells about the reject or accept state of acceptor.
    ///     DiverTo tells about the divertor state.
    ///     PendingDiverterAction tells about any pending action in divertor.
    ///     CoinTransmitTimer tells about transmit time
    /// </summary>
    public class CoinAcceptorState
    {
        internal void Reset()
        {
            State = AcceptorState.Reject;
            DivertTo = DivertorState.DivertToCashbox;
            PendingDiverterAction = DivertorAction.None;
            CoinTransmitTimer = 0;
        }

        internal DivertorState DivertTo { get; set; } = DivertorState.DivertToCashbox;
        internal AcceptorState State { get; set; } = AcceptorState.Reject;
        internal DivertorAction PendingDiverterAction { get; set; } = DivertorAction.None;
        internal Int64 CoinTransmitTimer { get; set; } = 0;
    }
}
