namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the progressive host is unavailable.
    /// </summary>
    public class ProgressiveHostOfflineEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Progressive Host Offline";
        }
    }
}
