namespace Aristocrat.Monaco.Hardware.Pwm.Hopper
{
    using System;

    internal sealed class CoinOutState
    {
        internal Int64 Timer { get; set; } = 0;
        internal HopperTaskState State { get; set; } = HopperTaskState.WaitingForLeadingEdge;
    }
}
