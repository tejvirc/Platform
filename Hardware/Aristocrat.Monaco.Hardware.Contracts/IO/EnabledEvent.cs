namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Globalization;
    using Kernel;
    using SharedDevice;

    /// <summary>Definition of the EnabledEvent class.</summary>
    [Serializable]
    public class EnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        [CLSCompliant(false)]
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        [CLSCompliant(false)]
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "IO {0} {1}",
                GetType().Name,
                Reasons);
        }
    }
}