namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    /// <summary>
    ///     Enum used to describe overlap or not Read option.
    /// </summary>
    public enum CreateFileOption
    {
        /// <summary> non overlap read </summary>
        None,
        /// <summary> overlap read </summary>
        Overlapped
    }
    /// <summary>
    ///     Class used to describe a pwm device config.
    /// </summary>
    public sealed class PwmDeviceConfig
    {
        /// <summary>
        ///     Get/set the mode
        /// </summary>
        public CreateFileOption Mode { get; set; }

        /// <summary>Provides the interface to connect with phyical device.</summary>
        /// <returns>Device interface.</returns>
        public System.Guid DeviceInterface { get; set; }

        /// <summary>
        ///     Polling frequnecy of device
        /// </summary>
        public int pollingFrequency { get; set; }

        /// <summary>
        ///     read/Write wait period on device
        /// </summary>
        public int waitPeriod { get; set; }
#pragma warning disable CS3003 // Argument type is not CLS-compliant
        /// <summary>
        ///     Type of PwmDevice device
        /// </summary>
        public uint DeviceType { get; set; }
#pragma warning restore CS3003 // Argument type is not CLS-compliant

    }
}
