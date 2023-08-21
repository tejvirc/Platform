namespace Aristocrat.Monaco.Gaming
{
    using Aristocrat.GdkRuntime.V1;
    using Hardware.Contracts.Reel;

    /// <summary>
    ///     A set of hardware reel extensions
    /// </summary>
    public static class HardwareReelExtensions
    {
        public static ReelState GetReelState(ReelLogicalState state)
        {
            var reelState = ReelState.Disconnected;

            switch (state)
            {
                case ReelLogicalState.Disconnected:
                    reelState = ReelState.Disconnected;
                    break;
                case ReelLogicalState.IdleAtStop:
                case ReelLogicalState.IdleUnknown:
                    reelState = ReelState.Stopped;
                    break;
                case ReelLogicalState.Spinning:
                case ReelLogicalState.SpinningForward:
                    reelState = ReelState.SpinningForward;
                    break;
                case ReelLogicalState.SpinningBackwards:
                    reelState = ReelState.SpinningBackwards;
                    break;
                case ReelLogicalState.Stopping:
                    reelState = ReelState.Stopping;
                    break;
                case ReelLogicalState.Tilted:
                    reelState = ReelState.Faulted;
                    break;
            }

            return reelState;
        }
    }
}