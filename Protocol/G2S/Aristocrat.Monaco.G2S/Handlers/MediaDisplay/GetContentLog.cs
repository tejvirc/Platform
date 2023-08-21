namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="getContentLog" /> command
    /// </summary>
    public class GetContentLog : ICommandHandler<mediaDisplay, getContentLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="GetContentLog" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public GetContentLog(IG2SEgm egm, IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, getContentLog> command)
        {
            return await Sanction.OwnerAndGuests<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, getContentLog> command)
        {
            var response = command.GenerateResponse<contentLogList>();

            response.Command.contentLog =
                _mediaProvider.MediaLog.ToList()
                    .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                    .Select(m => m.ToContentLog(_egm.GetDevice<IMediaDisplay>(m.PlayerId))).ToArray();

            await Task.CompletedTask;
        }
    }
}