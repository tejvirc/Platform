namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using Contracts.Models;
    using log4net;

    /// <summary>
    ///     Handles the GameSelected event, which launches the selected game
    /// </summary>
    public class GameSelectedConsumer : Consumes<GameSelectedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IGameService _gameService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectedConsumer" /> class.
        /// </summary>
        /// <param name="gameService">the game service</param>
        public GameSelectedConsumer(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <inheritdoc />
        public override void Consume(GameSelectedEvent theEvent)
        {
            var request = new GameInitRequest
            {
                GameId = theEvent.GameId,
                Denomination = theEvent.Denomination,
                BetOption = theEvent.BetOption,
                IsReplay = theEvent.IsReplay,
                GameBottomHwnd = theEvent.GameBottomHwnd,
                GameTopHwnd = theEvent.GameTopHwnd,
                GameVirtualButtonDeckHwnd = theEvent.GameVirtualButtonDeckHwnd,
                GameTopperHwnd = theEvent.GameTopperHwnd
            };

            Logger.Info("Calling Initialize from GameSelectedConsumer");
            _gameService.Initialize(request);
        }
    }
}
