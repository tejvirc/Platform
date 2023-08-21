namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    /// <summary>
    ///     Interface for builders of option config entries for specific device type.
    /// </summary>
    public interface IDeviceOptionsBuilder
    {
        /// <summary>
        ///     Matches the specified device class.
        /// </summary>
        /// <param name="deviceClass">The device class.</param>
        /// <returns>True is matches.</returns>
        bool Matches(DeviceClass deviceClass);

        /// <summary>
        ///     Builds the specified device options.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>g2s structure for representing options.</returns>
        deviceOptions Build(IDevice device, OptionListCommandBuilderParameters parameters);
    }
}