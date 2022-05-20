namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    /// <summary>
    ///     Defines the logical ids for the hardware devices.
    /// </summary>
    public enum ButtonLogicalId
    {
        /// <summary>
        ///     Defines the logical id for the main door.
        /// </summary>
        ButtonBase = 100,

        /// <summary>
        ///     Defines the Collect Button.
        /// </summary>
        Collect = ButtonBase + 1,

        /// <summary>
        ///     Defines the Play 1.
        /// </summary>
        Play1 = ButtonBase + 2,

        /// <summary>
        ///     Defines the Play 2.
        /// </summary>
        Play2 = ButtonBase + 3,

        /// <summary>
        ///     Defines the Play 3
        /// </summary>
        Play3 = ButtonBase + 4,

        /// <summary>
        ///     Defines the Bet 4.
        /// </summary>
        Play4 = ButtonBase + 5,

        /// <summary>
        ///     Defines the Play 5.
        /// </summary>
        Play5 = ButtonBase + 6,

        /// <summary>
        ///     Defines the Service.
        /// </summary>
        Service = ButtonBase + 7,

        /// <summary>
        ///     Defines the Bet 1.
        /// </summary>
        Bet1 = ButtonBase + 8,

        /// <summary>
        ///     Defines the Bet 2.
        /// </summary>
        Bet2 = ButtonBase + 9,

        /// <summary>
        ///     Defines the Bet 3
        /// </summary>
        Bet3 = ButtonBase + 10,

        /// <summary>
        ///     Defines the Bet 4.
        /// </summary>
        Bet4 = ButtonBase + 11,

        /// <summary>
        ///     Defines the Bet 5.
        /// </summary>
        Bet5 = ButtonBase + 12,

        /// <summary>
        ///     Defines the Play.
        /// </summary>
        Play = ButtonBase + 13,

        /// <summary>
        ///     Defines the TakeWin.
        /// </summary>
        TakeWin = ButtonBase + 14,

        /// <summary>
        ///     Defines the MaxBet.
        /// </summary>
        MaxBet = ButtonBase + 15,

        /// <summary>
        ///     Defines the GameRules.
        /// </summary>
        GameRules = ButtonBase + 16,

        /// <summary>
        ///     Defines the Help.
        /// </summary>
        Help = ButtonBase + 16,

        /// <summary>
        ///     Defines the Gamble.
        /// </summary>
        Gamble = ButtonBase + 17,

        /// <summary>
        ///     Defines the Bet6.
        /// </summary>
        Bet6 = ButtonBase + 18,

        /// <summary>
        ///     Defines the Bet7.
        /// </summary>
        Bet7 = ButtonBase + 19,

        /// <summary>
        ///     Defines the Bet8.
        /// </summary>
        Bet8 = ButtonBase + 20,

        /// <summary>
        ///     Defines the Bet9.
        /// </summary>
        Bet9 = ButtonBase + 21,

        /// <summary>
        ///     Defines the Bet10.
        /// </summary>
        Bet10 = ButtonBase + 22,

        /// <summary>
        ///     Defines the GameMenu.
        /// </summary>
        GameMenu = ButtonBase + 23,

        /// <summary>
        ///     Defines the ExitGame.
        /// </summary>
        ExitGame = ButtonBase + 23,

        /// <summary>
        ///     Defines the ExitToLobby.
        /// </summary>
        ExitToLobby = ButtonBase + 23,

        /// <summary>
        ///     Defines the Barkeeper button
        /// </summary>
        Barkeeper = ButtonBase + 24,

        /// <summary>
        ///     This is the Jackpot Switch
        /// </summary>
        Button30 = ButtonBase + 30,

        /// <summary>
        ///     Defines the BetDown Logical  button
        /// </summary>
        BetDown = ButtonBase + 31,

        /// <summary>
        ///     Defines the BetUp Logical  button
        /// </summary>
        BetUp = ButtonBase + 32,

        /// <summary>
        ///     Max Button Id
        /// </summary>
        MaxButtonId = BetUp
    }
}