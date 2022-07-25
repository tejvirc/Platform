namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Handles the GameRequestedPlatformHelpEvent
    /// </summary>
    public class GameRequestedPlatformHelpConsumer : Consumes<GameRequestedPlatformHelpEvent>
    {
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRequestedPlatformHelpEvent" /> class.
        /// </summary>
        /// <param name="runtime">An <see cref="IRuntime" /> instance.</param>
        public GameRequestedPlatformHelpConsumer(
            IRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <inheritdoc />
        public override void Consume(GameRequestedPlatformHelpEvent theEvent)
        {
            _runtime.UpdateFlag(RuntimeCondition.InPlatformHelp, theEvent.Visible);
        }
    }
}
