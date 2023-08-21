namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetIdReaderLocales : ICommandHandler<idReader, getIdReaderLocales>
    {
        private readonly IG2SEgm _egm;

        public GetIdReaderLocales(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<idReader, getIdReaderLocales> command)
        {
            return await Sanction.OwnerAndGuests<IIdReaderDevice>(_egm, command);
        }

        public Task Handle(ClassCommand<idReader, getIdReaderLocales> command)
        {
            var response = command.GenerateResponse<idReaderLocaleList>();

            response.Command.idReaderLocale = new idReaderLocale[] { };

            return Task.CompletedTask;
        }
    }
}
