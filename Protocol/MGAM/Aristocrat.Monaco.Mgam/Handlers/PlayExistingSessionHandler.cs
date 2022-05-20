namespace Aristocrat.Monaco.Mgam.Handlers
{
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Kernel;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handle the <see cref="PlayExistingSession" /> message.
    /// </summary>
    public class PlayExistingSessionHandler : MessageHandler<PlayExistingSession>
    {
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;

        /// <summary>
        ///     Construct a <see cref="PlayExistingSessionHandler" /> object.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="disableManager"><see cref="ISystemDisableManager"/>.</param>
        public PlayExistingSessionHandler(
            IEventBus eventBus,
            ISystemDisableManager disableManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        ///<inheritdoc />
        public override Task<IResponse> Handle(PlayExistingSession message)
        {
            _disableManager.Enable(MgamConstants.GamePlayDisabledKey);

            _eventBus.Publish(new PlayEvent(message.SessionId));

            return Task.FromResult(Ok<PlayExistingSessionResponse>());
        }
    }
}
