namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="getIpStatus"/> command
    /// </summary>
    public class GetIpStatus : ICommandHandler<informedPlayer, getIpStatus>
    {
        private readonly ICommandBuilder<IInformedPlayerDevice, ipStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="GetIpStatus"/>
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm"/> object</param>
        /// <param name="commandBuilder">A command builder for <see cref="ipStatus"/> response</param>
        public GetIpStatus(IG2SEgm egm, ICommandBuilder<IInformedPlayerDevice, ipStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<informedPlayer, getIpStatus> command)
        {
            return await Sanction.OnlyOwner<IInformedPlayerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<informedPlayer, getIpStatus> command)
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>(command.IClass.deviceId);
            device.HostActive = true;

            var response = command.GenerateResponse<ipStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
