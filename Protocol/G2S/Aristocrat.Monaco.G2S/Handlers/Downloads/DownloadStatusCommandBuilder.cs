namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IDownloadDevice, downloadStatus}" />
    /// </summary>
    public class DownloadStatusCommandBuilder : ICommandBuilder<IDownloadDevice, downloadStatus>
    {
        /// <inheritdoc />
        public async Task Build(IDownloadDevice device, downloadStatus command)
        {
            command.configComplete = device.ConfigComplete;

            if (device.ConfigDateTime != default(DateTime))
            {
                command.configDateTime = device.ConfigDateTime;
                command.configDateTimeSpecified = true;
            }

            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.egmLocked = device.Locked;
            command.hostEnabled = device.HostEnabled;

            if (!device.ListStateDateTime.HasValue)
            {
                command.lastProtocolChangeSpecified = false;
            }
            else
            {
                command.lastProtocolChangeSpecified = true;
                command.lastProtocolChange = device.ListStateDateTime.Value;
            }

            await Task.CompletedTask;
        }
    }
}