namespace Aristocrat.Monaco.Application.Contracts.EdgeLight
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the state of bottom strip has been turned off.
    /// </summary>
    public class BottomStripOffEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"{base.ToString()} {false}");
        }
    }
}