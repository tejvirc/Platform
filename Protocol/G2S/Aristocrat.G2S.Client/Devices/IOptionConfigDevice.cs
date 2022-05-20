namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with and control an optionConfig device.
    /// </summary>
    /// <typeparam name="TOptionChangeStatus">The type of the option change status.</typeparam>
    /// <seealso cref="Aristocrat.G2S.Client.Devices.IDevice" />
    /// <seealso cref="Aristocrat.G2S.Client.Devices.INoResponseTimer" />
    public interface IOptionConfigDevice<in TOptionChangeStatus> : IDevice, INoResponseTimer
    {
        /// <summary>
        ///     Gets minimum persisted log entries.
        /// </summary>
        int MinLogEntries { get; }

        /// <summary>
        ///     Options the change status.
        /// </summary>
        /// <param name="changeStatus">The change status.</param>
        void OptionChangeStatus(TOptionChangeStatus changeStatus);
    }
}