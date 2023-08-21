namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;

    /// <summary>
    ///     Provides a mechanism to build a 'commHostList' command.
    /// </summary>
    public interface ICommHostListCommandBuilder
    {
        /// <summary>
        ///     Builds/sets the attributes for the current command.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="command">The command to build.</param>
        /// <param name="parameters">Parameters for the builder.</param>
        /// <returns>A task.</returns>
        Task Build(ICommConfigDevice device, commHostList command, CommHostListCommandBuilderParameters parameters);
    }
}