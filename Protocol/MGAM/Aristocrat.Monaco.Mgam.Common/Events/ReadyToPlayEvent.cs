namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the ReadToPlayResponse message is received.
    /// </summary>
    public class ReadyToPlayEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName}]";
        }
    }
}
