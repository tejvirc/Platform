namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IEventHandlerDevice, eventHandlerStatus}" />
    /// </summary>
    public class EventHandlerStatusCommandBuilder : ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>
    {
        /// <inheritdoc />
        public async Task Build(IEventHandlerDevice device, eventHandlerStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.eventHandlerOverflow = device.Overflow;
            command.configComplete = device.ConfigComplete;

            if (device.ConfigDateTime != default(DateTime))
            {
                command.configDateTime = device.ConfigDateTime;
                command.configDateTimeSpecified = true;
            }

            await Task.CompletedTask;
        }
    }
}