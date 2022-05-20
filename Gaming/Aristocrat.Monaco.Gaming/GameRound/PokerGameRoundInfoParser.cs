namespace Aristocrat.Monaco.Gaming.GameRound
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using log4net;
    using Newtonsoft.Json;

    public class PokerGameRoundInfoParser : IGameRoundInfoParser
    {
        private const int MinimumTriggeredDataSize = 3;
        private const int DealOffset = 2;
        private const int DrawOffset = 3;
        private const int HoldOffset = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHandProvider _provider;
        private readonly IGamePlayState _gamePlayState;

        public PokerGameRoundInfoParser(IHandProvider provider, IGamePlayState gamePlayState)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
        }

        public string GameType => "poker";

        public string Version => "1";

        public void UpdateGameRoundInfo(IList<string> cardInformation)
        {
            try
            {
                if (!_gamePlayState.InGameRound || cardInformation.Count < MinimumTriggeredDataSize)
                {
                    return;
                }

                if (cardInformation.Count > DrawOffset)
                {
                    // held/draw information
                    _provider.UpdateHoldCards(JsonConvert.DeserializeObject<HeldCards>(cardInformation[HoldOffset]).Hold.ToList());
                    _provider.UpdateDrawCards(JsonConvert.DeserializeObject<DrawCards>(cardInformation[DrawOffset]).Draw.ToList());
                }
                else
                {
                    // initial deal information
                    _provider.UpdateDealtCards(JsonConvert.DeserializeObject<DealtCards>(cardInformation[DealOffset]).Deal.ToList());
                }
            }
            catch (JsonReaderException)
            {
                Logger.Error("Game sent bad JSON for the card information");
            }
        }
    }
}