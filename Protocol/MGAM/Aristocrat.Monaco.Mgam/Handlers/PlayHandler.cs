namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Handles <see cref="Play"/> message.
    /// </summary>
    public class PlayHandler : MessageHandler<Play>
    {
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;

        /// <summary>
        ///     Construct a <see cref="PlayHandler"/> object.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="disableManager"><see cref="ISystemDisableManager"/>.</param>
        public PlayHandler(IEventBus eventBus, ISystemDisableManager disableManager)
        {
            _eventBus = eventBus;
            _disableManager = disableManager;
        }

        ///<inheritdoc />
        public override Task<IResponse> Handle(Play message)
        {
            _disableManager.Enable(MgamConstants.GamePlayDisabledKey);
            _disableManager.Enable(MgamConstants.ConfiguringGamesGuid);

            _eventBus.Publish(new PlayEvent());

            return Task.FromResult(Ok<PlayResponse>());
        }
    }
}
