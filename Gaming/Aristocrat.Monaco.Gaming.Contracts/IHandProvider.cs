namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Contains methods to parse hand information received from
    ///     the game and provides information to the platform
    /// </summary>
    public interface IHandProvider
    {
        /// <summary>
        ///     Updates the Dealt Card information for the hand
        /// </summary>
        /// <param name="cards">The dealt cards</param>
        void UpdateDealtCards(List<Hand> cards);

        /// <summary>
        ///     Updates the Draw Card information for the hand
        /// </summary>
        /// <param name="cards">The draw cards</param>
        void UpdateDrawCards(List<Hand> cards);

        /// <summary>
        ///     Updates the Hold Card information for the hand
        /// </summary>
        /// <param name="cards">The hold cards</param>
        void UpdateHoldCards(List<Hand> cards);

        /// <summary>
        ///     Gets the list of cards that were held.
        ///     The first entry in the list corresponds to the left most card in the hand.
        ///     The last entry in the list corresponds to the right most card in the hand.
        ///     A 'Yes' value indicates the card was held.
        /// </summary>
        IList<HoldStatus> CardsHeld { get; }

        /// <summary>
        ///     Gets the list of cards in the current hand.
        ///     The first entry in the list corresponds to the left most card in the hand.
        ///     The last entry in the list corresponds to the right most card in the hand.
        /// </summary>
        IList<GameCard> CurrentHand { get; }

        /// <summary>
        ///     Gets a value indicating whether this is the final hand of the game or not
        /// </summary>
        bool FinalHand { get; }
    }

    /// <summary>
    ///     Used by Json deserializer to extract the hand information
    /// </summary>
    public class Hand
    {
        /// <summary> Gets or sets the row the cards were in </summary>
        public int Row { get; set; }

        /// <summary> Gets or sets the cards </summary>
        public long[] Cards { get; set; }
    }

    /// <summary>
    ///     Used by the json deserializer to extract the dealt cards information
    /// </summary>
    public class DealtCards
    {
        /// <summary> Gets or sets the dealt cards </summary>
        public Hand[] Deal { get; set; }
    }

    /// <summary>
    ///     Used by the json deserializer to extract the draw cards information
    /// </summary>
    public class DrawCards
    {
        /// <summary> Gets or sets the draw cards </summary>
        public Hand[] Draw { get; set; }
    }

    /// <summary>
    ///     Used by the json deserializer to extract the held cards information
    /// </summary>
    public class HeldCards
    {
        /// <summary> Gets or sets the held cards </summary>
        public Hand[] Hold { get; set; }
    }

    /// <summary>
    ///     Card held choices
    /// </summary>
    public enum HoldStatus
    {
        /// <summary> card is held </summary>
        Held,

        /// <summary> card is not held </summary>
        NotHeld
    }

    /// <summary>
    ///     Game card encoding.
    ///     The game sends us the card information encoded as follows:
    ///     card number = rank + (suit * CardInformationRank.Joker) for non-joker cards. Joker=52
    /// </summary>
    public enum GameCard
    {
        /// <summary>2 of Spades</summary>
        Spades2,

        /// <summary>3 of Spades</summary>
        Spades3,

        /// <summary>4 of Spades</summary>
        Spades4,

        /// <summary>5 of Spades</summary>
        Spades5,

        /// <summary>6 of Spades</summary>
        Spades6,

        /// <summary>7 of Spades</summary>
        Spades7,

        /// <summary>8 of Spades</summary>
        Spades8,

        /// <summary>9 of Spades</summary>
        Spades9,

        /// <summary>10 of Spades</summary>
        Spades10,

        /// <summary>Jack of Spades</summary>
        SpadesJack,

        /// <summary>Queen of Spades</summary>
        SpadesQueen,

        /// <summary>King of Spades</summary>
        SpadesKing,

        /// <summary>Ace of Spades</summary>
        SpadesAce,

        /// <summary>2 of Hearts</summary>
        Hearts2,

        /// <summary>3 of Hearts</summary>
        Hearts3,

        /// <summary>4 of Hearts</summary>
        Hearts4,

        /// <summary>5 of Hearts</summary>
        Hearts5,

        /// <summary>6 of Hearts</summary>
        Hearts6,

        /// <summary>7 of Hearts</summary>
        Hearts7,

        /// <summary>8 of Hearts</summary>
        Hearts8,

        /// <summary>9 of Hearts</summary>
        Hearts9,

        /// <summary>10 of Hearts</summary>
        Hearts10,

        /// <summary>Jack of Hearts</summary>
        HeartsJack,

        /// <summary>Queen of Hearts</summary>
        HeartsQueen,

        /// <summary>King of Hearts</summary>
        HeartsKing,

        /// <summary>Ace of Hearts</summary>
        HeartsAce,

        /// <summary>2 of Clubs</summary>
        Clubs2,

        /// <summary>3 of Clubs</summary>
        Clubs3,

        /// <summary>4 of Clubs</summary>
        Clubs4,

        /// <summary>5 of Clubs</summary>
        Clubs5,

        /// <summary>6 of Clubs</summary>
        Clubs6,

        /// <summary>7 of Clubs</summary>
        Clubs7,

        /// <summary>8 of Clubs</summary>
        Clubs8,

        /// <summary>9 of Clubs</summary>
        Clubs9,

        /// <summary>10 of Clubs</summary>
        Clubs10,

        /// <summary>Jack of Clubs</summary>
        ClubsJack,

        /// <summary>Queen of Clubs</summary>
        ClubsQueen,

        /// <summary>King of Clubs</summary>
        ClubsKing,

        /// <summary>Ace of Clubs</summary>
        ClubsAce,

        /// <summary>2 of Diamonds</summary>
        Diamonds2,

        /// <summary>3 of Diamonds</summary>
        Diamonds3,

        /// <summary>4 of Diamonds</summary>
        Diamonds4,

        /// <summary>5 of Diamonds</summary>
        Diamonds5,

        /// <summary>6 of Diamonds</summary>
        Diamonds6,

        /// <summary>7 of Diamonds</summary>
        Diamonds7,

        /// <summary>8 of Diamonds</summary>
        Diamonds8,

        /// <summary>9 of Diamonds</summary>
        Diamonds9,

        /// <summary>10 of Diamonds</summary>
        Diamonds10,

        /// <summary>Jack of Diamonds</summary>
        DiamondsJack,

        /// <summary>Queen of Diamonds</summary>
        DiamondsQueen,

        /// <summary>King of Diamonds</summary>
        DiamondsKing,

        /// <summary>Ace of Diamonds</summary>
        DiamondsAce,

        /// <summary>Joker</summary>
        Joker,

        /// <summary>Unknown card</summary>
        Unknown = 0xFF
    }
}