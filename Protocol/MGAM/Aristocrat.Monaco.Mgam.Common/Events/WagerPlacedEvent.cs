namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when a wager is placed
    /// </summary>
    public class WagerPlacedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WagerPlacedEvent"/> class.
        /// </summary>
        public WagerPlacedEvent(int penniesWagered)
        {
            PenniesWagered = penniesWagered;
        }

        /// <summary>
        ///     Gets the number of pennies wagered.
        /// </summary>
        public int PenniesWagered { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (Wagered: {PenniesWagered})]";
        }
    }
}
