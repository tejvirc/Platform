namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IAnalyticsDevice, analyticsProfile}" />
    /// </summary>
    public class AnalyticsProfileCommandBuilder : ICommandBuilder<IAnalyticsDevice, analyticsProfile>
    {
        public Task Build(IAnalyticsDevice device, analyticsProfile command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.configurationId = device.ConfigurationId;
            command.restartStatus = device.RestartStatus;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.requiredForPlay = device.RequiredForPlay;
            command.timeToLive = device.TimeToLive;
            command.noResponseTimer = device.NoResponseTimer;
            command.noMessageTimer = device.NoMessageTimer;
            command.noHostText = device.NoHostText;

            return Task.CompletedTask;
        }
    }
}