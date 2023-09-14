namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    using System;
    using Models;

    /// <summary>
    /// </summary>
    public interface ILobbyStateManager : IDisposable
    {
        /// <summary>
        ///     Property indicating the current state of the Lobby
        /// </summary>
        LobbyState CurrentState { get; }

        /// <summary>
        ///     Property indicating the previous state of the Lobby before the last state transition
        /// </summary>
        LobbyState PreviousState { get; }

        /// <summary>
        ///     Property indicating the lowest state in the State Queue
        /// </summary>
        LobbyState BaseState { get; }

        /// <summary>
        ///     
        /// </summary>
        LobbyCashOutState CashOutState { get; set; }

        /// <summary>
        /// ContainsAnyState return true if the State Queue contains ANY of the passed in states
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        bool ContainsAnyState(params LobbyState[] states);

        /// <summary>
        ///     Property indicating if the game runtime exited while we were disabled
        /// </summary>
        bool UnexpectedGameExitWhileDisabled { get; set; }

        /// <summary>
        ///     Property indicating if we are in process of loading a game for recovery
        /// </summary>
        bool IsLoadingGameForRecovery { get; }

        /// <summary>
        ///     Property indicating whether IsGameInCharge can ever be enabled.
        /// </summary>
        bool AllowGameInCharge { get; set; }

        /// <summary>
        ///     Property indicating whether games can auto-launch.
        /// </summary>
        bool AllowGameAutoLaunch { set; }

        /// <summary>
        ///     Property indicating a single playable game.
        /// </summary>
        bool IsSingleGame { set; }

        /// <summary>
        ///     Property indicating if there's just one playable game that takes priority over lobby.
        /// </summary>
        bool AllowSingleGameAutoLaunch { get; }

        /// <summary>
        ///     Property indicating if we are displaying the Tab View of the Lobby
        /// </summary>
        bool IsTabView { get; set; }

        /// <summary>
        ///     Property indicating if we are to reset the attract sequence on interruptions
        /// </summary>
        bool ResetAttractOnInterruption { get; set; }

        /// <summary>
        ///     Property indicating if attract mode can start. If no credits are present or we are in show mode, we can start the attract mode.
        /// </summary>
        bool CanAttractModeStart { get; }

        /// <summary>
        ///     Property indicating if cash out forced by max bank.
        /// </summary>
        bool IsCashoutForcedByMaxBank { get; }

        /// <summary>
        ///     Property indicating the last type of cash inserted (Currency vs Voucher vs Wat)
        /// </summary>
        CashInType LastCashInType { get; set; }

        /// <summary>
        ///     Callback called whenever there is a State entered
        /// </summary>
        Action<LobbyState, object> OnStateEntry { get; set; }

        /// <summary>
        ///     Callback called whenever there is a State exited
        /// </summary>
        Action<LobbyState, object> OnStateExit { get; set; }

        /// <summary>
        ///     Callback when we have received a Game Loaded message
        ///     while we are disabled
        /// </summary>
        Action GameLoadedWhileDisabled { get; set; }

        /// <summary>
        ///     Callback method that the Lobby State Manager
        ///     should call when it needs to manually update the
        ///     Lobby UI outside of a state transition
        /// </summary>
        Action UpdateLobbyUI { get; set; }

        /// <summary>
        ///     Callback method that the Lobby State Manager
        ///     should call when it needs to update the button lamps
        ///     outside of a state transition
        /// </summary>
        Action UpdateLamps { get; set; }

        /// <summary>
        ///     Callback to see if we need to put up an Age Warning dialog
        ///     immediately after initial cash in
        /// </summary>
        Func<AgeWarningCheckResult> CheckForAgeWarning { get; set; }

        /// <summary>
        ///     Initializes LobbyStateManager
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Fires a trigger for the Lobby State Manager
        ///     Triggers generally change the state, but can be used
        ///     to initiate other in-state actions as well
        /// </summary>
        /// <param name="trigger">The Trigger to fire</param>
        /// <param name="parameter">optional param</param>
        void SendTrigger(LobbyTrigger trigger, object parameter = null);

        /// <summary>
        ///     Function indicating if the Lobby is "in" the passed in state
        ///     THis can be useful when used with sub-states.  For example
        ///     there are several states that are sub-states of the Disabled
        ///     state or the Chooser state, so you can pass in
        ///     IsInState(LobbyState.Disabled) and any state that is a sub-state
        ///     of LobbyState.Disabled will return true
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        bool IsInState(LobbyState state);

        /// <summary>
        ///     To be called when there is user interaction with the lobby
        ///     This sets a variable in the Lobby State Manager and
        ///     controls state flow in certain cases (Attract Mode)
        /// </summary>
        void OnUserInteraction();

        /// <summary>
        ///     Identifies if the State is one of the Responsible Gaming info States
        ///     I would rather do this through Stateless but it doesn't seem to have 
        ///     a function for this unless you are checking the current state.
        /// </summary>
        bool IsStateInResponsibleGamingInfo(LobbyState state);

        /// <summary>
        ///     AddStackableState adds a state directly into the queue. 
        ///     Used in very specific situations to directly alter the state queue from outside the State Manager
        /// </summary>
        /// <param name="state"></param>
        void AddStackableState(LobbyState state);

        /// <summary>
        ///     RemoveStackableState removes a state directly from the queue. 
        ///     Used in very specific situations to directly alter the state queue from outside the State Manager
        /// </summary>
        /// <param name="state"></param>
        void RemoveStackableState(LobbyState state);

        /// <summary>
        ///     AddFlagState should be used to trigger flag states instead of using a Trigger
        /// </summary>
        /// <param name="state"></param>
        /// <param name="param"></param>
        /// /// <param name="param2"></param>
        void AddFlagState(LobbyState state, object param = null, object param2 = null);

        /// <summary>
        ///     RemoveFlagState should be used to un-trigger flag states instead of using a Trigger
        /// </summary>
        /// <param name="state"></param>
        /// <param name="param"></param>
        void RemoveFlagState(LobbyState state, object param = null);
    }
}
