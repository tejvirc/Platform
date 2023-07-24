namespace Aristocrat.Monaco.Hardware.Pwm.CoinAcceptor
{
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using System;
 

    public class CoinAcceptorState
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
