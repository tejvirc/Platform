namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     The card encoding per the SAS spec.
    /// </summary>
    public enum SasCard
    {
        Spades2 = 0,
        Spades3,
        Spades4,
        Spades5,
        Spades6,
        Spades7,
        Spades8,
        Spades9,
        Spades10,
        SpadesJack,
        SpadesQueen,
        SpadesKing,
        SpadesAce,

        Clubs2 = 0x10,
        Clubs3,
        Clubs4,
        Clubs5,
        Clubs6,
        Clubs7,
        Clubs8,
        Clubs9,
        Clubs10,
        ClubsJack,
        ClubsQueen,
        ClubsKing,
        ClubsAce,

        Hearts2 = 0x20,
        Hearts3,
        Hearts4,
        Hearts5,
        Hearts6,
        Hearts7,
        Hearts8,
        Hearts9,
        Hearts10,
        HeartsJack,
        HeartsQueen,
        HeartsKing,
        HeartsAce,

        Diamonds2 = 0x30,
        Diamonds3,
        Diamonds4,
        Diamonds5,
        Diamonds6,
        Diamonds7,
        Diamonds8,
        Diamonds9,
        Diamonds10,
        DiamondsJack,
        DiamondsQueen,
        DiamondsKing,
        DiamondsAce,

        Joker = 0x4D,
        Other = 0x5E,
        Unknown = 0x5E
    }

    /// <inheritdoc />
    public class SendCardInformationResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets or sets whether the hand type is Final Hand
        /// </summary>
        public bool FinalHand { get; set; }

        /// <summary>
        ///     Gets or sets card 1
        /// </summary>
        public SasCard Card1 { get; set; }

        /// <summary>
        ///     Gets or sets card 2
        /// </summary>
        public SasCard Card2 { get; set; }

        /// <summary>
        ///     Gets or sets card 3
        /// </summary>
        public SasCard Card3 { get; set; }

        /// <summary>
        ///     Gets or sets card 4
        /// </summary>
        public SasCard Card4 { get; set; }

        /// <summary>
        ///     Gets or sets card 5
        /// </summary>
        public SasCard Card5 { get; set; }
    }
}