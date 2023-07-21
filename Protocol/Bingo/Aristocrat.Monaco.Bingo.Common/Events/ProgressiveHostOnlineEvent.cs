namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the progressive host is available.
    /// </summary>
    public class ProgressiveHostOnlineEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Progressive Host Online";
        }
    }
}
