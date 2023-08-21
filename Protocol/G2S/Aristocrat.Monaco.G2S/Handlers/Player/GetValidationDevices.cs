namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts.IdReader;

    public class GetValidationDevices : ICommandHandler<player, getValidationDevices>
    {
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly IG2SEgm _egm;

        public GetValidationDevices(IG2SEgm egm, IIdReaderProvider idReaderProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
        }

        public async Task<Error> Verify(ClassCommand<player, getValidationDevices> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getValidationDevices> command)
        {
            var devices = new List<validationDevice1>();

            var response = command.GenerateResponse<validationDeviceList>();

            var playerDevice = _egm.GetDevice<IPlayerDevice>();

            var idReaders = _idReaderProvider.Adapters.ToList();

            if (playerDevice.IdReader != 0)
            {
                devices.Add(
                    new validationDevice1
                    {
                        idReaderId = playerDevice.IdReader,
                        idReaderType = idReaders.FirstOrDefault(i => i.IdReaderId == playerDevice.IdReader)?.IdReaderType.ToIdReaderType(),
                        idReaderLinked = true
                    });
            }

            if (playerDevice.UseMultipleIdDevices)
            {
                // This must include everything else, but the one added above (if non-zero)
                devices.AddRange(
                    idReaders.Where(idReader => idReader.IdReaderId != playerDevice.IdReader)
                        .Select(
                            idReader => new validationDevice1
                            {
                                idReaderId = idReader.IdReaderId,
                                idReaderType = idReader.IdReaderType.ToIdReaderType(),
                                idReaderLinked = playerDevice.IdReaders.Any(id => id == idReader.IdReaderId)
                            }));
            }

            response.Command.validationDevice = devices.ToArray();

            await Task.CompletedTask;
        }
    }
}
