namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    /// <summary>Game category names</summary>
    public enum GameCategory
    {
        /// <summary> Undefined </summary>
        Undefined = -1,
        /// <summary> LightningLink </summary>
        LightningLink,
        /// <summary> Slot </summary>
        Slot,
        /// <summary> Keno </summary>
        Keno,
        /// <summary> Poker </summary>
        Poker,
        /// <summary> Table </summary>
        Table,
        /// <summary> MultiDrawPoker </summary>
        MultiDrawPoker
    }

    /// <summary>Game subcategory names</summary>
    public enum GameSubCategory
    {
        /// <summary> Undefined </summary>
        Undefined = -1,
        /// <summary> OneHand </summary>
        OneHand,
        /// <summary> ThreeHand </summary>
        ThreeHand,
        /// <summary> FiveHand </summary>
        FiveHand,
        /// <summary> TenHand </summary>
        TenHand,
        /// <summary> SingleCard </summary>
        SingleCard,
        /// <summary> FourCard </summary>
        FourCard,
        /// <summary> MultiCard </summary>
        MultiCard,
        /// <summary> BlackJack </summary>
        BlackJack,
        /// <summary> Roulette </summary>
        Roulette
    }
}
