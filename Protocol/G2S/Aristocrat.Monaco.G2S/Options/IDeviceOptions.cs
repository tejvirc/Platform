namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using Monaco.Common.Validation;

    /// <summary>
    ///     Interface for device-specific components that controls the logic of setting the values to device options.
    /// </summary>
    public interface IDeviceOptions
    {
        /// <summary>
        ///     Matches the specified device class.
        /// </summary>
        /// <param name="deviceClass">The device class.</param>
        /// <returns>True is matches.</returns>
        bool Matches(DeviceClass deviceClass);

        /// <summary>
        ///     Applies the properties.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="optionConfigValues">The option configuration values.</param>
        void ApplyProperties(IDevice device, DeviceOptionConfigValues optionConfigValues);

        /// <summary>
        ///     Validates option values.
        /// </summary>
        /// <param name="option">Option to validate.</param>
        /// <returns>Returns error or null.</returns>
        ValidationResult Verify(Option option);
    }
}