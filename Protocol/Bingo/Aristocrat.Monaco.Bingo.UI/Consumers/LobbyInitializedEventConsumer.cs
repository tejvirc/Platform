namespace Aristocrat.Monaco.Bingo.UI.Consumers
{
    using System;
    using System.Collections.Generic;
    using Bingo.Consumers;
    using Common;
    using Common.Events;
    using Gaming.Contracts.Lobby;
    using Kernel;
    using Loaders;

    public class LobbyInitializedEventConsumer : Bingo.Consumers.Consumes<LobbyInitializedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IBingoDisplayConfigurationProvider _configurationProvider;
        private readonly IDispatcher _dispatcher;
        private readonly IEnumerable<IBingoPresentationLoader> _presentationLoaders;

        public LobbyInitializedEventConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IBingoDisplayConfigurationProvider configurationProvider,
            IDispatcher dispatcher,
            IEnumerable<IBingoPresentationLoader> presentationLoaders)
            : base(eventBus, sharedConsumer)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _presentationLoaders = presentationLoaders ?? throw new ArgumentNullException(nameof(presentationLoaders));
        }

        public override void Consume(LobbyInitializedEvent theEvent)
        {
            _dispatcher.ExecuteOnUIThread(
                () =>
                {
                    _configurationProvider.LobbyInitialized();
                    foreach (var loader in _presentationLoaders)
                    {
                        loader.LoadPresentation();
                    }

                    _eventBus.Publish(new BingoDisplayConfigurationStartedEvent());
                });
        }
    }
}