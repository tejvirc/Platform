namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetIdReaderProfile : ICommandHandler<idReader, getIdReaderProfile>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderProfile> _commandBuilder;
        private readonly IG2SEgm _egm;

        public GetIdReaderProfile(IG2SEgm egm, ICommandBuilder<IIdReaderDevice, idReaderProfile> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<idReader, getIdReaderProfile> command)
        {
            return await Sanction.OwnerAndGuests<IIdReaderDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<idReader, getIdReaderProfile> command)
        {
            var device = _egm.GetDevice<IIdReaderDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<idReaderProfile>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
