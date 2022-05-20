namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="GamePlayInitiatedEvent" /> event.
    ///     This class resets the poker hand information used by LP8E when
    ///     a new game starts. The hand information will be filled in
    ///     by game messages if it is a poker game.
    /// </summary>
    public class GamePlayInitiatedConsumer : Consumes<GamePlayInitiatedEvent>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly HandInformation _defaultHandInformation = new ();

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayInitiatedConsumer" /> class.
        /// </summary>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager" /></param>
        public GamePlayInitiatedConsumer(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(GamePlayInitiatedEvent gamePlayInitiated)
        {
            _propertiesManager.SetProperty(GamingConstants.PokerHandInformation, _defaultHandInformation);
        }
    }
}