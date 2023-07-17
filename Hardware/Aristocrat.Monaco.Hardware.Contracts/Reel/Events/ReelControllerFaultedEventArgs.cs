namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The event arguments for reel controller fault events
    /// </summary>
    public class ReelControllerFaultedEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelControllerFaultedEventArgs"/>
        /// </summary>
        /// <param name="faults">The faults that were for this event</param>
        public ReelControllerFaultedEventArgs(ReelControllerFaults faults)
        {
            Faults = faults;
        }

        /// <summary>
        ///     Gets the faults for this events
        /// </summary>
        public ReelControllerFaults Faults { get; }
    }
}