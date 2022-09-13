namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    using Contracts.Gds.Reel;

    public static class GlobalStatusExtensions
    {
        public static FailureStatus ToFailureStatus(this GlobalStatus status)
        {
            return new FailureStatus
            {
                MechanicalError = (status & GlobalStatus.ReelVoltageLow) == GlobalStatus.ReelVoltageLow,
                HardwareError = (status & GlobalStatus.LampVoltageLow) == GlobalStatus.LampVoltageLow
            };
        }
    }
}