namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the Unplay command is received.
    /// </summary>
    public class UnplayEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName}]";
        }
    }
}
