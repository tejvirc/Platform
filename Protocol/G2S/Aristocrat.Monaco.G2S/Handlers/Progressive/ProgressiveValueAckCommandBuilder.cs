namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class ProgressiveValueAckCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveValueAck>
    {
        public async Task Build(IProgressiveDevice device, progressiveValueAck command)
        {
            checkForNullInput(device, command);

            await Task.CompletedTask;
        }

        private void checkForNullInput(IProgressiveDevice device, progressiveValueAck command)
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
