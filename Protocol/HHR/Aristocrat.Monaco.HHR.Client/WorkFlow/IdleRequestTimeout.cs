namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using Messages;

    /// <summary>
    ///     Defines attributes when no action required on timeout.
    /// </summary>
    public class IdleRequestTimeout : IRequestTimeout
    {
        /// <inheritdoc />
        public TimeoutBehaviorType TimeoutBehaviorType => TimeoutBehaviorType.Idle;
    }
}