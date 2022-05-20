namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.EventHandler;
    using Data.Model;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Handles the v21.getEventHandlerLogStatus G2S message
    /// </summary>
    public class GetEventHandlerLogStatus : ICommandHandler<eventHandler, getEventHandlerLogStatus>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventHandlerLogRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetEventHandlerLogStatus" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="repository">Repository</param>
        /// <param name="contextFactory">Context factory.</param>
        public GetEventHandlerLogStatus(
            IG2SEgm egm,
            IEventHandlerLogRepository repository,
            IMonacoContextFactory contextFactory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getEventHandlerLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getEventHandlerLogStatus> command)
        {
            var response = command.GenerateResponse<eventHandlerLogStatus>();

            using (var context = _contextFactory.Create())
            {
                response.Command.totalEntries = _repository.Count(context, l => l.HostId == command.HostId);
                response.Command.lastSequence = response.Command.totalEntries > 0
                    ? _repository.GetMaxLastSequence<EventHandlerLog>(context, l => l.HostId == command.HostId)
                    : 0;
            }

            await Task.CompletedTask;
        }
    }
}