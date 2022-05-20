namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The event arguments for reel fault events
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    public class ReelFaultedEventArgs : ReelEventArgs
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelFaultedEventArgs"/>
        /// </summary>
        /// <param name="faults">The faults that were for this event</param>
        /// <param name="reelId">The reel Id for this fault</param>
        public ReelFaultedEventArgs(ReelFaults faults, int reelId)
            : base(reelId)
        {
            Faults = faults;
        }

        /// <summary>
        ///     Gets the faults for this events
        /// </summary>
        public ReelFaults Faults { get; }
    }
}