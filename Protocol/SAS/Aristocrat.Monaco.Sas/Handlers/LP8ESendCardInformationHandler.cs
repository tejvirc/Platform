namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     The handler for LP 8E Send Card Information
    /// </summary>
    public class LP8ESendCardInformationHandler : ISasLongPollHandler<SendCardInformationResponse, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly HandInformation _defaultHandInformation = new HandInformation();
        private readonly Dictionary<GameCard, SasCard> _cardConverter = new Dictionary<GameCard, SasCard>();

        /// <summary>
        ///     Creates an Instance of the LP8ESendCardInformationHandler
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        public LP8ESendCardInformationHandler(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));

            // fill the dictionary with the game card to sas card mappings
            foreach (var card in Enum.GetNames(typeof(GameCard)))
            {
                var sasCard = (SasCard)Enum.Parse(typeof(SasCard), card);
                var gameCard = (GameCard)Enum.Parse(typeof(GameCard), card);
                _cardConverter.Add(gameCard, sasCard);
            }
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendCardInformation
        };

        /// <inheritdoc />
        public SendCardInformationResponse Handle(LongPollData data)
        {
            // get last card information the game sent us
            var handInformation = _propertiesManager.GetValue(GamingConstants.PokerHandInformation, _defaultHandInformation);
            return new SendCardInformationResponse
            { 
                FinalHand = handInformation.FinalHand,
                Card1 = ConvertFromGameCardToSasCard(handInformation.Cards[0]),
                Card2 = ConvertFromGameCardToSasCard(handInformation.Cards[1]),
                Card3 = ConvertFromGameCardToSasCard(handInformation.Cards[2]),
                Card4 = ConvertFromGameCardToSasCard(handInformation.Cards[3]),
                Card5 = ConvertFromGameCardToSasCard(handInformation.Cards[4]),
            };
        }

        private SasCard ConvertFromGameCardToSasCard(GameCard card)
        {
            return _cardConverter.TryGetValue(card, out var sasCard) ? sasCard : SasCard.Unknown;
        }
    }
}