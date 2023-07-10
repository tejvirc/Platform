namespace Aristocrat.Monaco.Hardware.Contracts.ButtonDeck
{
    using Button;

    /// <summary>
    /// LcdButtonDeckLobby maps LCD Button Deck Logical Ids to their corresponding lobby function
    /// </summary>
    public enum LcdButtonDeckLobby
    {
        /// <summary>NextGame</summary>
        NextGame = ButtonLogicalId.Play4,

        /// <summary>NextTab</summary>
        NextTab = ButtonLogicalId.Play5,

        /// <summary>CashOut</summary>
        CashOut = ButtonLogicalId.Collect,

        /// <summary>PreviousGame</summary>
        PreviousGame = ButtonLogicalId.Bet3,

        /// <summary>PreviousTab</summary>
        PreviousTab = ButtonLogicalId.Bet4,

        /// <summary>ChangeDenom</summary>
        ChangeDenom = ButtonLogicalId.Bet5,

        /// <summary>LaunchGame</summary>
        LaunchGame = ButtonLogicalId.Play,

        /// <summary>DualLaunchGame</summary>
        DualLaunchGame = ButtonLogicalId.DualPlay
    }
}