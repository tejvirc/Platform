namespace Aristocrat.Monaco.Hardware.Contracts.Battery
{
    using Kernel;
    using System.Globalization;

    /// <summary>
    ///     Definition of the BatteryLowEvent class
    /// </summary>
    public class BatteryLowEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BatteryLowEvent" /> class.
        /// </summary>
        /// <param name="batteryId">The Id of the low battery</param>
        public BatteryLowEvent(int batteryId)
        {
            BatteryId = batteryId;
        }

        /// <summary>
        ///     Gets the BatteryId of the error event
        /// </summary>
        public int BatteryId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"Battery {BatteryId + 1} {GetType().Name}");
        }
    }
}