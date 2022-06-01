namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using V1;

    /// <summary>
    ///     Command handler for the <see cref="GetReelState" /> command.
    /// </summary>
    public class GetReelStateCommandHandler : ICommandHandler<GetReelState>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReelController _reelController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetReelStateCommandHandler" /> class.
        /// </summary>
        public GetReelStateCommandHandler()
        {
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
        }

        /// <inheritdoc />
        public void Handle(GetReelState command)
        {
            Logger.Debug("Handle GetReelState command");

            foreach (var reelState in _reelController?.ReelStates ?? Enumerable.Empty<KeyValuePair<int, ReelLogicalState>>())
            {
                switch (reelState.Value)
                {
                    case ReelLogicalState.Disconnected:
                        command.States.Add(reelState.Key, ReelState.Disconnected);
                        break;
                    case ReelLogicalState.IdleAtStop:
                    case ReelLogicalState.IdleUnknown:
                        command.States.Add(reelState.Key, ReelState.Stopped);
                        break;
                    case ReelLogicalState.Homing:
                    case ReelLogicalState.Spinning:
                    case ReelLogicalState.SpinningForward:
                        command.States.Add(reelState.Key, ReelState.SpinningForward);
                        break;
                    case ReelLogicalState.SpinningBackwards:
                        command.States.Add(reelState.Key, ReelState.SpinningBackwards);
                        break;
                    case ReelLogicalState.Stopping:
                        command.States.Add(reelState.Key, ReelState.Stopping);
                        break;
                    case ReelLogicalState.Tilted:
                        command.States.Add(reelState.Key, ReelState.Faulted);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}