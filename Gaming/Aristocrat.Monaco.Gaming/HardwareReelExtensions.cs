namespace Aristocrat.Monaco.Gaming
{
    using Kernel;
    using GdkRuntime.V1;
    using Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;

    /// <summary>
    ///     A set of hardware reel extensions
    /// </summary>
    public static class HardwareReelExtensions
    {
        public static ReelState GetReelState(ReelLogicalState state)
        {
            var reelController = ServiceManager.GetInstance().GetService<IReelController>();
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
                    reelState = reelController.HasCapability<IReelSpinCapabilities>() ?
                        ReelState.SpinningForward : ReelState.SpinningConstant;
                    break;
                case ReelLogicalState.SpinningForward:
                    reelState = ReelState.SpinningForward;
                    break;
                case ReelLogicalState.SpinningBackwards:
                    reelState = ReelState.SpinningBackwards;
                    break;
                case ReelLogicalState.SpinningConstant:
                    reelState = ReelState.SpinningConstant;
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