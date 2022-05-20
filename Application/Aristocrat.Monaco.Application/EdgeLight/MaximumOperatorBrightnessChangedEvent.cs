namespace Aristocrat.Monaco.Application.EdgeLight
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the edge light brightness has been changed by the operator.
    /// </summary>
    public class MaximumOperatorBrightnessChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MaximumOperatorBrightnessChangedEvent" /> class.
        /// </summary>
        /// <param name="brightness">The max brightness set by the operator</param>
        public MaximumOperatorBrightnessChangedEvent(int brightness)
        {
            MaxOperatorBrightness = brightness;
        }

        /// <summary>
        ///     Gets the max brightness set by the operator
        /// </summary>
        public int MaxOperatorBrightness { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"{base.ToString()} {MaxOperatorBrightness}");
        }
    }
}