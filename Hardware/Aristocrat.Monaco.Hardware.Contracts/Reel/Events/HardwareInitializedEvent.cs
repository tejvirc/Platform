namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The initialized event for a given reel controller
    /// </summary>
    /// <remarks>
    ///     The Initialized Event is posted by the Reel Controller service
    /// </remarks>
    [Serializable]
    public class HardwareInitializedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareInitializedEvent" /> class.
        /// </summary>
        public HardwareInitializedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareInitializedEvent" /> class.
        /// </summary>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public HardwareInitializedEvent(int reelControllerId)
            : base(reelControllerId)
        {
        }
    }
}
