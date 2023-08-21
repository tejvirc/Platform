namespace Aristocrat.Monaco.G2S.Handlers.Gat
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
    public class GetGatLogStatus : ICommandHandler<gat, getGatLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGatLogStatus" /> class.
        ///     Constructs a new instance using an egm and the GAT service.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        public GetGatLogStatus(IG2SEgm egm, IGatService gatService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getGatLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getGatLogStatus> command)
        {
            var response = command.GenerateResponse<gatLogStatus>();

            var result = _gatService.GetLogStatus();
            response.Command.lastSequence = result.LastSequence;
            response.Command.totalEntries = result.TotalEntries;

            await Task.CompletedTask;
        }
    }
}