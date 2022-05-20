namespace Aristocrat.Monaco.Application.Contracts.FirmwareCrcMonitor
{
    using Kernel;
    using Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    /// An event when hardware CRC mismatch is detected.
    /// </summary>
    public class FirmwareCrcMismatchedEvent : BaseEvent
    {
        private readonly string _hardwareNames;

        /// <summary>
        /// ctor for unit test purpose
        /// </summary>
        public FirmwareCrcMismatchedEvent()
        {
        }

        /// <summary>
        /// ctor for FirmwareCrcMismatchedEvent
        /// </summary>
        /// <param name="hardwareNames">Hardware name that has firmware CRC mismatch issue.</param>
        public FirmwareCrcMismatchedEvent(string hardwareNames)
        {
            _hardwareNames = hardwareNames;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.FirmwareCrcMismatchedEvent, _hardwareNames);
        }
    }
}
