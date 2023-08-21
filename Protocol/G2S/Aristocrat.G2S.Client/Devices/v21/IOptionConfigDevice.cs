namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a commConfig device.
    /// </summary>
    public interface IOptionConfigDevice : IOptionConfigDevice<optionChangeStatus>
    {
        /// <summary>
        ///     Used to notify the host that EGM options have changed
        /// </summary>
        /// <param name="options">The list of changed options</param>
        void OptionsChanged(deviceOptions[] options);
    }
}