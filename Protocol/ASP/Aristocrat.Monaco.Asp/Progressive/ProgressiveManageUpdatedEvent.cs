namespace Aristocrat.Monaco.Asp.Progressive
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     An event that notifies progressive-related data sources that the progressive manager has had it's model updated.
    /// </summary>
    public class ProgressiveManageUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// Initial values for use by JackpotNumberAndControllerIdDataSource
        /// </summary>
        public IReadOnlyDictionary<int, JackpotNumberAndControllerIdState> JackpotNumberAndControllerIds { get; }

        public ProgressiveManageUpdatedEvent(IReadOnlyDictionary<int, JackpotNumberAndControllerIdState> jackpotNumberAndControllerIds)
        {
            JackpotNumberAndControllerIds = jackpotNumberAndControllerIds;
        }
    }
}