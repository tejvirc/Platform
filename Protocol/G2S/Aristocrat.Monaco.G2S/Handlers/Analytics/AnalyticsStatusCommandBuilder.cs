namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IAnalyticsDevice, analyticsStatus}" />
    /// </summary>
    public class AnalyticsStatusCommandBuilder : ICommandBuilder<IAnalyticsDevice, analyticsStatus>
    {
        /// <inheritdoc />
        public Task Build(IAnalyticsDevice device, analyticsStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;

            return Task.CompletedTask;
        }
    }
}
