namespace Aristocrat.Monaco.Hardware.Contracts.Dfu
{
    using System.Globalization;
    using SharedDevice;
    using Kernel;

    /// <summary>Definition of the DfuDownloadStartEvent class.</summary>
    /// <remarks>This event is posted to trigger a DFU download for a given device type.</remarks>
    public class DfuDownloadStartEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DfuDownloadStartEvent" /> class.
        /// </summary>
        /// <param name="device">Type of device.</param>
        public DfuDownloadStartEvent(DeviceType device)
        {
            Device = device;
        }

        /// <summary>Gets the type of device.</summary>
        public DeviceType Device { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Device={1}]",
                GetType().Name,
                Device);
        }
    }
}