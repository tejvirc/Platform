namespace Aristocrat.Monaco.Hardware.PWM
{
    using System;
    using Contracts.PWM;

    internal class CoinAcceptorState
    {
        internal void Reset()
        {
            State = AcceptorState.Reject;
            //DivertTo = DivertorState.None;
            DivertTo = DivertorState.DivertToCashbox;
            pendingDiverterAction = DivertorAction.None;
            CoinTransmitTimer = 0;

        }
        internal DivertorState DivertTo { get; set; } = DivertorState.DivertToCashbox;
        internal AcceptorState State { get; set; } = AcceptorState.Reject;
        internal DivertorAction pendingDiverterAction { get; set; } = DivertorAction.None;
        internal Int64 CoinTransmitTimer { get; set; } = 0;
    }
}
