namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;

    /// <summary>
    ///     Provides a mechanism to build a 'optionList' command.
    /// </summary>
    public interface IOptionListCommandBuilder
    {
        /// <summary>
        ///     Builds/sets the attributes for the current command.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="command">The command to build.</param>
        /// <param name="parameters">Parameters for the builder.</param>
        /// <returns>A task.</returns>
        Task Build(IOptionConfigDevice device, optionList command, OptionListCommandBuilderParameters parameters);
    }
}