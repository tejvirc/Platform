namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A GameRequestedPlatformHelpEvent should be posted when game requests to begin/end platform help
    /// </summary>
    public class GameRequestedPlatformHelpEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="visible"></param>
        public GameRequestedPlatformHelpEvent(bool visible)
        {
            Visible = visible;
        }

        /// <summary>
        ///     Visibility
        /// </summary>
        public bool Visible { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GetType().Name} - {Visible}";
        }
    }
}