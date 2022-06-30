﻿namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;

    internal class StateChecker
    {
        private readonly ILobbyStateManager _lobbyStateManager;
        public IGamePlayState _gamePlayState;

        public StateChecker(ILobbyStateManager lobbyStateManager, IGamePlayState gamePlayState)
        {
            _lobbyStateManager = lobbyStateManager;
            _gamePlayState = gamePlayState;
        }
        #region GamePlayState
        internal bool IsIdle => _gamePlayState.CurrentState == PlayState.Idle;
        internal bool IsGameEnded => _gamePlayState.CurrentState == PlayState.GameEnded;
        internal bool IsPrimaryGameEnded => _gamePlayState.CurrentState == PlayState.PrimaryGameEnded;
        internal bool IsSecondaryGameEnded => _gamePlayState.CurrentState == PlayState.SecondaryGameEnded;
        internal bool IsPresentationIdle => _gamePlayState.CurrentState == PlayState.PresentationIdle;
        
        #endregion
        #region LobbyStateManager
        internal bool IsAllowSingleGameAutoLaunch => _lobbyStateManager.AllowSingleGameAutoLaunch;
        internal bool IsStartup => _lobbyStateManager.CurrentState == LobbyState.Startup;
        internal bool IsChooser => _lobbyStateManager.CurrentState == LobbyState.Chooser;
        internal bool IsChooserScrollingIdleText => _lobbyStateManager.CurrentState == LobbyState.ChooserScrollingIdleText;
        internal bool IsChooserIdleTextTimer => _lobbyStateManager.CurrentState == LobbyState.ChooserIdleTextTimer;
        internal bool IsAttract => _lobbyStateManager.CurrentState == LobbyState.Attract;
        internal bool IsGameLoading => _lobbyStateManager.CurrentState == LobbyState.GameLoading;
        internal bool IsGameLoadingForDiagnostics => _lobbyStateManager.CurrentState == LobbyState.GameLoadingForDiagnostics;
        internal bool IsGame => _lobbyStateManager.CurrentState == LobbyState.Game;
        internal bool IsGameDiagnostics => _lobbyStateManager.CurrentState == LobbyState.GameDiagnostics;
        internal bool IsResponsibleGamingInfo => _lobbyStateManager.CurrentState == LobbyState.ResponsibleGamingInfo;
        internal bool IsResponsibleGamingInfoLayeredLobby => _lobbyStateManager.CurrentState == LobbyState.ResponsibleGamingInfoLayeredLobby;
        internal bool IsResponsibleGamingInfoLayeredGame => _lobbyStateManager.CurrentState == LobbyState.ResponsibleGamingInfoLayeredGame;
        internal bool IsCashOut => _lobbyStateManager.CurrentState == LobbyState.CashOut;
        internal bool IsCashOutFailure => _lobbyStateManager.CurrentState == LobbyState.CashOutFailure;
        internal bool IsDisabled => _lobbyStateManager.CurrentState == LobbyState.Disabled;
        internal bool IsRecovery => _lobbyStateManager.CurrentState == LobbyState.Recovery;
        internal bool IsRecoveryFromStartup => _lobbyStateManager.CurrentState == LobbyState.RecoveryFromStartup;
        internal bool IsResponsibleGamingTimeLimitDialog => _lobbyStateManager.CurrentState == LobbyState.ResponsibleGamingTimeLimitDialog;
        internal bool IsAgeWarningDialog => _lobbyStateManager.CurrentState == LobbyState.AgeWarningDialog;
        internal bool IsPrintHelpline => _lobbyStateManager.CurrentState == LobbyState.PrintHelpline;
        internal bool IsCashIn => _lobbyStateManager.CurrentState == LobbyState.CashIn;
        internal bool IsMediaPlayerOverlay => _lobbyStateManager.CurrentState == LobbyState.MediaPlayerOverlay;
        internal bool IsMediaPlayerResizing => _lobbyStateManager.CurrentState == LobbyState.MediaPlayerResizing;


        internal bool IsInRecovery => IsRecovery || IsRecoveryFromStartup;
#endregion
    }
}
