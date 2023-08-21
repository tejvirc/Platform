namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///     Provides a mechanism to control responsible gaming.
    /// </summary>
    public interface IResponsibleGaming : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     Gets the number of sessions allowed before cashout.  This varies based on jurisdiction.  A
        ///     session limit less than 1 means there is no limit.
        /// </summary>
        int SessionLimit { get; }

        /// <summary> Gets a value indicating whether there are session limits. </summary>
        bool HasSessionLimits { get; }

        /// <summary> Gets a value indicating whether Responsible Gaming is enabled. </summary>
        bool Enabled { get; }

        /// <summary> Gets the current session count. </summary>
        int SessionCount { get; }

        /// <summary>
        ///     Gets a value indicating whether the Session Limit has been hit
        /// </summary>
        bool IsSessionLimitHit { get; }

        /// <summary> Gets a value indicating whether the time limit dialog is visible. </summary>
        bool IsTimeLimitDialogVisible { get; }

        /// <summary>
        ///     Gets the state of the time limit dialog currently being shown
        /// </summary>
        ResponsibleGamingDialogState TimeLimitDialogState { get; }

        /// <summary>
        ///     Gets the Resource Key of the Responsible Gaming Dialog to currently display
        /// </summary>
        string ResponsibleGamingDialogResourceKey { get; }

        /// <summary>
        ///     Gets a value indicating whether the user has a responsible gaming decision pending to be made
        ///     aka this value will be true until they have properly dismissed the dialog with a choice.
        /// </summary>
        bool ShowTimeLimitDlgPending { get; }

        /// <summary>
        ///     Gets the current Responsible Gaming Mode.  Only works if Responsible Gaming has been initialized
        /// </summary>
        ResponsibleGamingMode ResponsibleGamingMode { get; }

        /// <summary>
        ///     Remaining time in the current Responsible Gaming session
        /// </summary>
        TimeSpan RemainingSessionTime { get; }

        /// <summary>
        ///     Initialize.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Call this to show the Responsible Gaming dialog
        ///     if inGame is true, the RuntimeFlag.TimeoutImminent will be fired first and IsTimeLimitDialogVisible will be set 1
        ///     second later
        /// </summary>
        /// <param name="allowDialogWhileDisabled">
        ///     allows the dialog to come up even if the system is disabled.  Only used when
        ///     bringing dialog back up after Audit Menu
        /// </param>
        void ShowDialog(bool allowDialogWhileDisabled);

        /// <summary>
        ///     Used to load properties from persistent storage.
        /// </summary>
        void LoadPropertiesFromPersistentStorage();

        /// <summary>
        ///     User accepted a time limit choice.
        /// </summary>
        /// <param name="timeLimitIndex">The property index of the time limit choice.</param>
        void AcceptTimeLimit(int timeLimitIndex);

        /// <summary>
        ///     Handles the initial currency-in.
        /// </summary>
        void OnInitialCurrencyIn();

        /// <summary>
        ///     Called to end a Responsible Gaming Session
        /// </summary>
        void EndResponsibleGamingSession();

        /// <summary> Handles game play disabled. </summary>
        void OnGamePlayDisabled();

        /// <summary> Handles cashing out started. </summary>
        void OnGamePlayEnabled();

        /// <summary>
        ///     Event to signal that Responsible Gaming is Forcing a Cash Out on the machine
        /// </summary>
        event EventHandler ForceCashOut;

        /// <summary>
        ///     Resets the dialog state from showing to pending.  Should only be called in bad situation where the dialog is already
        ///     up but we need
        ///     back the dialog out to do something, like a recovery.
        /// </summary>
        void ResetDialog(bool resetDueToOperatorMenu);

        /// <summary>
        /// Tells the Runtime if we can play games or not because an RG dialog is pending or up
        /// </summary>
        bool CanSpinReels();

        /// <summary>
        /// Locks the RG dialog from coming up for X milliseconds.  This closes the vulnerability
        /// between when the runtime asks for a reel spin and we say "Yes, you can spin the reels"
        /// and when we actually get into a GamePlay Initiated State
        /// </summary>
        void EngageSpinGuard();

        /// <summary>
        /// SpinGuard property indicates if the SpinGuard is active, 
        /// which will prevent the RG dialog from being activated by the lobby
        /// </summary>
        bool SpinGuard { get; }

        /// <summary>
        /// Event for Responsible Gaming State Changes
        /// </summary>
        event ResponsibleGamingStateChangeEventHandler OnStateChange;

        /// <summary>
        /// Event for forcing a check for Pending Responsible Gaming Dialogs
        /// </summary>
        event EventHandler<EventArgs> OnForcePendingCheck;
    }
}
