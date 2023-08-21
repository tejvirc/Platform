namespace Aristocrat.Monaco.G2S.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Provides a mechanism to build a G2S command.
    /// </summary>
    /// <typeparam name="TDevice">The device type.</typeparam>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public interface ICommandBuilder<in TDevice, in TCommand>
        where TDevice : IDevice
        where TCommand : ICommand
    {
        /// <summary>
        ///     Builds/sets the attributes for the current command.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="command">The command to build.</param>
        /// <returns>A task.</returns>
        Task Build(TDevice device, TCommand command);
    }
}