namespace Aristocrat.Monaco.Application.Contracts.EdgeLight
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the state of bottom strip has been turned on.
    /// </summary>
    public class BottomStripOnEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"{base.ToString()} {true}");
        }
    }
}