namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.setKeepAlive G2S message
    /// </summary>
    public class SetKeepAlive : ICommandHandler<communications, setKeepAlive>
    {
        private const int MinimumKeepAliveValue = 5 * 1000;

        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetKeepAlive" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public SetKeepAlive(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, setKeepAlive> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            if (command.Command.interval < MinimumKeepAliveValue && command.Command.interval != 0)
            {
                return new Error(ErrorCode.G2S_CMX001);
            }

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwner));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, setKeepAlive> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                device.SetKeepAlive(command.Command.interval);

                command.GenerateResponse<setKeepAliveAck>();
            }

            await Task.CompletedTask;
        }
    }
}