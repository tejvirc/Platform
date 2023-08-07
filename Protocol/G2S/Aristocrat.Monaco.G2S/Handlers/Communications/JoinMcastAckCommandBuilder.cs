namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class JoinMcastAckCommandBuilder : ICommandBuilder<ICommunicationsDevice, joinMcastAck>
    {
        /// <inheritdoc />
        public async Task Build(ICommunicationsDevice device, joinMcastAck command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await Task.CompletedTask;
        }
    }
}
