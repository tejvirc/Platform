namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Handler for the <see cref="masterReset" /> command
    /// </summary>
    public class MasterReset : ICommandHandler<cabinet, masterReset>
    {
        private readonly IG2SEgm _egm;
        private readonly IMasterResetService _service;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MasterReset" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="service">Master reset service.</param>
        public MasterReset(
            IG2SEgm egm,
            IMasterResetService service
        )
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, masterReset> command)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (!device.MasterResetAllowed)
            {
                return new Error("GTK_CBX004");
            }

            if (_service.HasMasterReset())
            {
                return new Error("GTK_CBX003");
            }

            return await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, masterReset> command)
        {
            await _service.Handle(command);
        }
    }
}