namespace Aristocrat.G2S.Client.Communications
{
    /// <summary>
    ///     Describes the communication behavior for outbound communications
    /// </summary>
    public interface ICommBehavior : ITimeoutBehavior
    {
        /// <summary>
        ///     Gets the retry count for a G2S message
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        ///     Gets the time to live behavior
        /// </summary>
        TimeToLiveBehavior TimeToLiveBehavior { get; }
    }
}