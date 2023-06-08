namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using System.ComponentModel;
    using Lobby;

    /// <summary>
    ///     Area to list the string in this window.
    /// </summary>
    public enum InfoLocation
    {
        /// <summary> TopLeft </summary>
        TopLeft,

        /// <summary> TopRight </summary>
        TopRight,

        /// <summary> BotLeft </summary>
        BotLeft,

        /// <summary> BotRight </summary>
        BotRight
    }

    /// <summary> Enum for the various States that the Lobby can be in </summary>
    public enum LobbyState
    {
        /// <summary> Initial state of the Lobby when the system boots </summary>
        Startup,

        /// <summary> Game Chooser Screen, with no scrolling idle text </summary>
        Chooser,

        /// <summary> Game Chooser Screen, with scrolling idle text </summary>
        ChooserScrollingIdleText,

        /// <summary> Game Chooser Screen, waiting to display scrolling idle text </summary>
        ChooserIdleTextTimer,

        /// <summary> Attract Mode </summary>
        Attract,

        /// <summary> Game Loading </summary>
        GameLoading,

        /// <summary> Game Loading for Diagnostics </summary>
        [StackableLobbyState] GameLoadingForDiagnostics,

        /// <summary> Game Playing </summary>
        Game,

        /// <summary> Game Diagnostics </summary>
        [StackableLobbyState] GameDiagnostics,

        /// <summary> Responsible Gaming Info Dialogs Displayed </summary>
        [StackableLobbyState] ResponsibleGamingInfo,

        /// <summary> Responsible Gaming Info Dialogs Displayed From Responsible Gaming Dialog Over Lobby </summary>
        [StackableLobbyState] ResponsibleGamingInfoLayeredLobby,

        /// <summary> Responsible Gaming Info Dialogs Displayed From Responsible Gaming Dialog Over Game </summary>
        [StackableLobbyState] ResponsibleGamingInfoLayeredGame,

        /// <summary> Cashing Out Message Displayed </summary>
        [StackableLobbyState][FlagLobbyState] CashOut,

        /// <summary> Cashing Out Failure Message </summary>
        [StackableLobbyState][FlagLobbyState] CashOutFailure,

        /// <summary> Disabled (Lockup) State </summary>
        [StackableLobbyState] Disabled,

        /// <summary> Recovery Mode </summary>
        Recovery,

        /// <summary> Recovery Mode From Startup(no visible Lobby) </summary>
        RecoveryFromStartup,

        /// <summary> Responsible Gaming Timeout Dialog Displayed </summary>
        [StackableLobbyState] ResponsibleGamingTimeLimitDialog,

        /// <summary> Age Warning Dialog </summary>
        [StackableLobbyState] AgeWarningDialog,

        /// <summary> Print Helpline Message Displayed </summary>
        [StackableLobbyState] PrintHelpline,

        /// <summary> Cash In Message </summary>
        [StackableLobbyState][FlagLobbyState] CashIn,

        /// <summary> Cash In Message </summary>
        [StackableLobbyState][FlagLobbyState] MediaPlayerOverlay,

        /// <summary> Media Viewer Animation </summary>
        [StackableLobbyState][FlagLobbyState] MediaPlayerResizing
    }

    /// <summary> Enum for the various Triggers that can drive action in the Lobby </summary>
    public enum LobbyTrigger
    {
        /// <summary> Enters the Lobby from startup </summary>
        LobbyEnter,

        /// <summary> Attract Timer has fired </summary>
        AttractTimer,

        /// <summary> Attract Video has completed </summary>
        AttractVideoComplete,

        /// <summary> Attract Mode has been exited by user input </summary>
        AttractModeExit,

        /// <summary> Game is being started </summary>
        LaunchGame,

        /// <summary> Game is being started for Diagnostics </summary>
        LaunchGameForDiagnostics,

        /// <summary> Game has finished loading </summary>
        GameLoaded,

        /// <summary> Game exited normally </summary>
        GameNormalExit,

        /// <summary> Game exited unexpectedly </summary>
        GameUnexpectedExit,

        /// <summary> Game in Diagnostics Mode has exited </summary>
        GameDiagnosticsExit,

        /// <summary> Responsible Gaming Info Button was pressed </summary>
        ResponsibleGamingInfoButton,

        /// <summary> Responsible Gaming Info was exited by user </summary>
        ResponsibleGamingInfoExit,

        /// <summary> Responsible Gaming Info timed out </summary>
        ResponsibleGamingInfoTimeOut,

        /// <summary> Responsible Gaming Time Limit Dialog is going to display </summary>
        ResponsibleGamingTimeLimitDialog,

        /// <summary> Responsible Gaming Time Limit Dialog has been dismissed </summary>
        ResponsibleGamingTimeLimitDialogDismissed,

        /// <summary> System has been disabled </summary>
        Disable,

        /// <summary> System has been Enabled </summary>
        Enable,

        /// <summary> Recovery is being initiated </summary>
        InitiateRecovery,

        /// <summary> Idle Text scrolling has completed </summary>
        IdleTextScrollingComplete,

        /// <summary> Idle Text timer has triggered </summary>
        IdleTextTimer,

        /// <summary> Age Warning Dialog has been triggered </summary>
        AgeWarningDialog,

        /// <summary> Age Warning Dialog has timed out </summary>
        AgeWarningTimeout,

        /// <summary> Print Helpline Ticket </summary>
        PrintHelpline,

        /// <summary> Print Helpline Finished </summary>
        PrintHelplineComplete,

        /// <summary> Changes the Lobby Idle text Display State to Static</summary>
        SetLobbyIdleTextStatic,

        /// <summary> Changes the Lobby Idle text Display State to Scrolling</summary>
        SetLobbyIdleTextScrolling,

        /// <summary> Return to lobby button pressed</summary>
        ReturnToLobby
    }

    /// <summary>
    /// Enum indicating the types of cash for the cash-in process
    /// </summary>
    public enum CashInType
    {
        /// <summary> Currency (cash money) </summary>
        Currency,

        /// <summary> Voucher (ticket) </summary>
        Voucher,

        /// <summary> Wat </summary>
        Wat
    }

    /// <summary> Defines the game type enum </summary>
    public enum GameType
    {
        /// <summary> game type is undefined </summary>
        [Description("Undefined")]
        Undefined = -1,

        /// <summary> game type is a slot based game </summary>
        [Description("Slot")]
        Slot = 0,

        /// <summary> game type is a poker game </summary>
        [Description("Poker")]
        Poker,

        /// <summary> game type is a keno game </summary>
        [Description("Keno")]
        Keno,

        /// <summary> game type is a blackjack game </summary>
        [Description("Blackjack")]
        Blackjack,

        /// <summary> game type is a roulette game </summary>
        [Description("Roulette")]
        Roulette,

        /// <summary> game type is a Lightning Link game </summary>
        [Description("LightningLink")]
        LightningLink
    }

    /// <summary> Defines the game icon type enum </summary>
    public enum GameIconType
    {
        /// <summary> game icon type uses default icon</summary>
        [Description("Default")]
        Default = 0,

        /// <summary> game icon type uses icon with no progressive information</summary>
        [Description("NoProgressiveIcon")]
        NoProgressiveInformation,

        /// <summary> game icon type uses icon with progressive value</summary>
        [Description("ProgressiveValue")]
        ProgressiveValue,

        /// <summary> game icon type uses icon with progressive label</summary>
        [Description("ProgressiveLabel")]
        ProgressiveLabel
    }

    /// <summary>
    ///     Game Hold Percentage type used for game setup
    /// </summary>
    public enum GameHoldPercentageType
    {
        /// <summary> Off </summary>
        Off,

        /// <summary> Low </summary>
        Low,

        /// <summary> Medium </summary>
        Medium,

        /// <summary> High </summary>
        High
    }

    /// <summary>
    ///     Defines the lobby VBD video state.
    /// </summary>
    public enum LobbyVbdVideoState
    {
        /// <summary> InsertMoney </summary>
        InsertMoney,

        /// <summary> ChooseGame </summary>
        ChooseGame,

        /// <summary> ChooseTime </summary>
        ChooseTime,

        /// <summary> Disabled </summary>
        Disabled
    }

    /// <summary>
    /// Enum for the Result of the AgeWarningCheck
    /// </summary>
    public enum AgeWarningCheckResult
    {
        /// <summary> False </summary>
        False,
        /// <summary> True </summary>
        True,
        /// <summary> DisableDeferred </summary>
        DisableDeferred
    }

    /// <summary>
    /// Enum for the Lobby Cash Out State
    /// </summary>
    public enum LobbyCashOutState
    {
        /// <summary> Undefined </summary>
        Undefined,
        /// <summary> Wat </summary>
        Wat,
        /// <summary> Voucher </summary>
        Voucher,
        /// <summary> HandPay </summary>
        HandPay
    }

    /// <summary>
    /// Enum to track the Lobby Cash Out Dialog State
    /// </summary>
    public enum LobbyCashOutDialogState
    {
        /// <summary> Hidden </summary>
        Hidden,
        /// <summary> Dialog is visible, neither timeout or completed event have occurred </summary>
        Visible,
        /// <summary> Dialog is visible, completed event has occured, awaiting timeout to hide </summary>
        VisiblePendingTimeout,
        /// <summary> Dialog is visible, timeout has occurred, awaiting completed event to hide </summary>
        VisiblePendingCompletedEvent
    }

    /// <summary>
    /// Enum to track Message Overlay State
    /// </summary>
    public enum MessageOverlayState
    {
        /// <summary> Disabled </summary>
        Disabled,
        /// <summary> CashOut </summary>
        CashOut,
        /// <summary> VoucherNotification </summary>
        VoucherNotification,
        /// <summary> PrintHelpline </summary>
        PrintHelpline,
        /// <summary> CashOutFailure </summary>
        CashOutFailure,
        /// <summary> CashIn </summary>
        CashIn,
        /// <summary> Handpay </summary>
        Handpay,
        /// <summary> Diagnostics </summary>
        Diagnostics,
        /// <summary> Progressive Game Disabled Notification </summary>
        ProgressiveGameDisabledNotification
    }

    /// <summary>
    /// Enum for determining which sound effect is played
    /// </summary>
    public enum Sound
    {
        /// <summary> Touch </summary>
        Touch,

        /// <summary> CoinIn </summary>
        CoinIn,

        /// <summary> CoinOut </summary>
        CoinOut,

        /// <summary> FeatureBell </summary>
        FeatureBell,

        /// <summary> Collect </summary>
        Collect,

        /// <summary> Paper in chute sound </summary>
        PaperInChute,
    }

    /// <summary>
    /// Defines the options available to operator on dialogue during large win pay method
    /// if selected payment method is MenuSelection
    /// </summary>
    public enum MenuSelectionPayOption
    {
        /// <summary>
        ///     Option to return back to lockup.
        /// </summary>
        ReturnToLockup,
        /// <summary>
        ///     Option to pay by hand.
        /// </summary>
        PayByHand,
        /// <summary>
        ///     Option to pay to credit.
        /// </summary>
        PayToCredit,
    }

    /// <summary>
    ///     Enum Helper Class
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Enum en) where T : Attribute
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(T), false);
                if (attrs.Length > 0)
                {
                    return (T)attrs[0];
                }
            }

            return null;
        }
    }
}
