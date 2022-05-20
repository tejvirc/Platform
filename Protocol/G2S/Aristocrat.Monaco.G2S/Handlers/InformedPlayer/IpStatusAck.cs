namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handle the <see cref="IpStatusAck"/> response from host
    /// </summary>
    public class IpStatusAck : ICommandHandler<informedPlayer, ipStatusAck>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="IpStatusAck"/>
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm"/> object</param>
        public IpStatusAck(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<informedPlayer, ipStatusAck> command)
        {
            return await Sanction.OnlyOwner<IInformedPlayerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public Task Handle(ClassCommand<informedPlayer, ipStatusAck> command)
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>(command.IClass.deviceId);
            device.HostActive = true;

            return Task.CompletedTask;
        }
    }
}
