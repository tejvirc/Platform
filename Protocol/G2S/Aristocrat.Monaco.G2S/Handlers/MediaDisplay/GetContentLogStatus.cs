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
    ///     Handle the <see cref="getContentLogStatus" /> command
    /// </summary>
    public class GetContentLogStatus : ICommandHandler<mediaDisplay, getContentLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="GetContentLogStatus" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public GetContentLogStatus(IG2SEgm egm, IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, getContentLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, getContentLogStatus> command)
        {
            var mediaList = _mediaProvider.MediaLog.ToList();

            var response = command.GenerateResponse<contentLogStatus>();

            response.Command.lastSequence = 0;
            response.Command.totalEntries = mediaList.Count;

            if (response.Command.totalEntries > 0)
            {
                response.Command.lastSequence = mediaList.Last().LogSequence;
            }

            await Task.CompletedTask;
        }
    }
}