namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using Common.PerformanceCounters;
    using Runtime;

    /// <summary>
    ///     Command handler for the <see cref="ReplayGameEnded" /> command.
    /// </summary>
    [CounterDescription("Replay Game End", PerformanceCounterType.AverageTimer32)]
    public class ReplayGameEndedCommandHandler : ICommandHandler<ReplayGameEnded>
    {
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReplayGameEndedCommandHandler" /> class.
        /// </summary>
        public ReplayGameEndedCommandHandler(IRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <inheritdoc />
        public void Handle(ReplayGameEnded command)
        {
            _runtime.UpdateBalance(command.EndCredits);
        }
    }
}
