namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetTrackIntervalAckCommandBuilder : ICommandBuilder<IAnalyticsDevice, setTrackIntervalAck>
    {
        public Task Build(IAnalyticsDevice device, setTrackIntervalAck command)
        {
            CheckForNullInput(device, command);

            return Task.CompletedTask;
        }

        private static void CheckForNullInput(IAnalyticsDevice device, setTrackIntervalAck command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
        }
    }
}
