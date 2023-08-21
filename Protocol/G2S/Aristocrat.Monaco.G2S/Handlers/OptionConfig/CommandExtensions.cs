namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     The commands extensions.
    /// </summary>
    internal static class CommandExtensions
    {
        /// <summary>
        ///     Checks whether the getOptionList command includes all devices.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>True if command includes all devices otherwise false.</returns>
        public static bool IncludeAllDevices(this c_deviceSelect command)
        {
            return command.deviceClass.Equals(DeviceClass.G2S_all);
        }
    }
}