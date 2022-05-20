namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using Messages;

    /// <summary>
    ///     Interface responsible for all Timeout Behavior Types
    /// </summary>
    public interface IRequestTimeout
    {
        /// <summary>
        ///     Timeout Behavior Type
        /// </summary>
        TimeoutBehaviorType TimeoutBehaviorType { get; }
    }
}