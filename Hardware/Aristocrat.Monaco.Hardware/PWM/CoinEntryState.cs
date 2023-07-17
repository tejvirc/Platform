namespace Aristocrat.Monaco.Hardware.PWM
{
    using System;
    using Contracts.PWM;

    internal sealed class CoinEntryState
    {
        internal void Reset()
        {
            SenseTime = 0;
            SenseState = SenseSignalState.HighToLow;
            SensePulses = 0;
            CreditTime = 0;
            CreditState = CreditSignalState.HighToLow;
            AlarmTime = 0;
            AlarmState = AlarmSignalState.HighToLow;
            SenseToCreditTime = 0;
            DivertingTo = DivertorState.None;
            currentState = Cc62Signals.None;
        }

        internal Int64 SenseTime { get; set; } = 0;
        internal SenseSignalState SenseState { get; set; } = SenseSignalState.HighToLow;
        internal Int64 SensePulses { get; set; } = 0;
        internal Int64 CreditTime { get; set; } = 0;
        internal CreditSignalState CreditState { get; set; } = CreditSignalState.HighToLow;
        internal Int64 AlarmTime { get; set; } = 0;
        internal AlarmSignalState AlarmState { get; set; } = AlarmSignalState.HighToLow;
        internal Int64 SenseToCreditTime { get; set; } = 0;
        internal DivertorState DivertingTo { get; set; } = DivertorState.None;
        internal Cc62Signals currentState { get; set; } = Cc62Signals.None;
    }
}
