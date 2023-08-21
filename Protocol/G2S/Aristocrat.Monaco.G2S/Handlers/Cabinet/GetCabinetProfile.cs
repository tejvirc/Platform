namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetCabinetProfile : ICommandHandler<cabinet, getCabinetProfile>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetProfile> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCabinetProfile" /> class using an egm and the property manager.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="commandBuilder">An ICommandBuilder instance.</param>
        public GetCabinetProfile(IG2SEgm egm, ICommandBuilder<ICabinetDevice, cabinetProfile> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, getCabinetProfile> command)
        {
            return await Sanction.OwnerAndGuests<ICabinetDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, getCabinetProfile> command)
        {
            var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<cabinetProfile>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}