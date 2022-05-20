namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Globalization;
    using Kernel;
    using SharedDevice;

    /// <summary>Definition of the DisabledEvent class.</summary>
    [Serializable]
    public class DisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        [CLSCompliant(false)]
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        [CLSCompliant(false)]
        public DisabledReasons Reasons { get; }

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