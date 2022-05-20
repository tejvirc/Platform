namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    public static class ReelStatusExtensions
    {
        public static bool IsReelConnected(this ReelStatus status)
        {
            return (status & (ReelStatus.RmsConnected | ReelStatus.RmConnected)) > 0;
        }

        public static Contracts.Gds.Reel.ReelStatus ToReelStatus(this ReelStatus status, int reelId, bool ignoreConnected)
        {
            return new Contracts.Gds.Reel.ReelStatus
            {
                ReelId = reelId,
                Connected = status.IsReelConnected() || ignoreConnected,
                ReelStall = (status & ReelStatus.Stalled) == ReelStatus.Stalled,
                ReelTampered = (status & ReelStatus.ReelOutOfSync) == ReelStatus.ReelOutOfSync,
                RequestError = (status & ReelStatus.BadValue) == ReelStatus.BadValue ||
                               (status & ReelStatus.ReelInError) == ReelStatus.ReelInError ||
                               (status & ReelStatus.ReelAlreadySpinning) == ReelStatus.ReelAlreadySpinning ||
                               (status & ReelStatus.BadState) == ReelStatus.BadState ||
                               (status & ReelStatus.ReelOutOfSync) == ReelStatus.ReelOutOfSync ||
                               (status & ReelStatus.ReelNotAvailable) == ReelStatus.ReelNotAvailable ||
                               (status & ReelStatus.GameChecksum) == ReelStatus.GameChecksum ||
                               (status & ReelStatus.FaultChecksum) == ReelStatus.FaultChecksum,
                LowVoltage = (status & ReelStatus.ReelVoltageLow) == ReelStatus.ReelVoltageLow
            };
        }
    }
}