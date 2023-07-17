namespace Aristocrat.Monaco.Hardware.PWM
{
    internal enum CoinAcceptorCommands
    {
        CoinAcceptorRecordCount = 1,
        CoinAcceptorRejectOn,
        CoinAcceptorRejectOff,
        CoinAcceptorDivertorOn,
        CoinAcceptorDivertorOff,
        CoinAcceptorRegisterValue,
        CoinAcceptorPollingCount,
        CoinAcceptorPeek,
        CoinAcceptorAcknowledge,
        CoinAcceptorSetInputRegister,
        CoinAcceptorStartPolling,
        CoinAcceptorStopPolling
    }
    internal enum Cc62Signals
    {
        None,
        SenseSignal = (1 << 2),
        CreditSignal = (1 << 3),
        AlarmSignal = (1 << 4),
        SolenoidSignal = (1 << 5)
    }

    internal enum SenseSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }

    internal enum CreditSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }

    internal enum AlarmSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }
    internal enum DivertorAction
    {
        None,
        DivertToHopper,
        DivertToCashbox,
    }
    internal enum CoinEntryStatus : byte
    {
        Read,
        InProcess,
        Ack
    }
    internal class CoinSignalsConsts
    {
        /* Coin entry input masks */

        /* Sense pulse lengths
            0..4    = noise
            5..40   = valid
            41...   = sesnse fault
        */
        internal const uint SensePulseMin = 5;       /* Min Sense pulse length */
        internal const uint SensePulseMax = 40;      /* Max Sense pulse length */

        /* Credit pulse lengths
            0..4    = noise
            5..40   = valid
            41...   = credit fault
        */
        internal const uint CreditPulseMin = 5;       /* Min Credit pulse length */
        internal const uint CreditPulseMax = 40;      /* Max Credit pulse length */

        /* Alarm pulse lengths
            0..5    = noise
            6..240  = yoyo
            241...  = coin in optic fault
        */
        internal const uint AlarmPulseMin = 5;       /* Min Credit pulse length */
        internal const uint AlarmPulseMax = 240;     /* Max Credit pulse length */

        internal const uint SenseToCreditMaxtime = 250;     /* Max allowed time from sense to credit pulse */

        internal const uint CoinTransitTime = 1000;
    }
}
