namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;

    internal class StateChecker
    {
        public readonly IGamePlayState _gamePlayState;

        public StateChecker(IGamePlayState gamePlayState)
        {
            _gamePlayState = gamePlayState;
        }
        internal bool IsIdle => _gamePlayState.CurrentState == PlayState.Idle;
        internal bool IsInitiated => _gamePlayState.CurrentState == PlayState.Initiated;
        internal bool IsPrimaryGameEscrow => _gamePlayState.CurrentState == PlayState.PrimaryGameEscrow;
        internal bool IsPrimaryGameStarted => _gamePlayState.CurrentState == PlayState.PrimaryGameStarted;
        internal bool IsPrimaryGameEnded => _gamePlayState.CurrentState == PlayState.PrimaryGameEnded;
        internal bool IsProgressivePending => _gamePlayState.CurrentState == PlayState.ProgressivePending;
        internal bool IsSecondaryGameChoice => _gamePlayState.CurrentState == PlayState.SecondaryGameChoice;
        internal bool IsSecondaryGameEscrow => _gamePlayState.CurrentState == PlayState.SecondaryGameEscrow;
        internal bool IsSecondaryGameStarted => _gamePlayState.CurrentState == PlayState.SecondaryGameStarted;
        internal bool IsSecondaryGameEnded => _gamePlayState.CurrentState == PlayState.SecondaryGameEnded;
        internal bool IsPayGameResults => _gamePlayState.CurrentState == PlayState.PayGameResults;
        internal bool IsFatalError => _gamePlayState.CurrentState == PlayState.FatalError;
        internal bool IsGameEnded => _gamePlayState.CurrentState == PlayState.GameEnded;
        internal bool IsPresentationIdle => _gamePlayState.CurrentState == PlayState.PresentationIdle;
        internal bool IsInTheMiddleOfPlaying => IsPrimaryGameEscrow ||
                                                IsSecondaryGameEscrow ||
                                                IsPrimaryGameStarted ||
                                                IsProgressivePending ||
                                                IsSecondaryGameStarted ||
                                                IsPayGameResults ||
                                                IsGameEnded;

        internal bool IsAllowSingleGameAutoLaunch { get; set; }
        internal bool IsStartup { get; set; }
        internal bool IsChooser { get; set; }
        internal bool IsChooserScrollingIdleText { get; set; }
        internal bool IsChooserIdleTextTimer { get; set; }
        internal bool IsAttract { get; set; }
        internal bool IsGameLoading { get; set; }
        internal bool IsGameLoadingForDiagnostics { get; set; }
        internal bool IsGame { get; set; }
        internal bool IsGameDiagnostics { get; set; }
        internal bool IsResponsibleGamingInfo { get; set; }
        internal bool IsResponsibleGamingInfoLayeredLobby { get; set; }
        internal bool IsResponsibleGamingInfoLayeredGame { get; set; }
        internal bool IsCashOut { get; set; }
        internal bool IsCashOutFailure { get; set; }
        internal bool IsDisabled { get; set; }
        internal bool IsRecovery { get; set; }
        internal bool IsRecoveryFromStartup { get; set; }
        internal bool IsResponsibleGamingTimeLimitDialog { get; set; }
        internal bool IsAgeWarningDialog { get; set; }
        internal bool IsPrintHelpline { get; set; }
        internal bool IsCashIn { get; set; }
        internal bool IsMediaPlayerOverlay { get; set; }
        internal bool IsMediaPlayerResizing { get; set; }
        internal bool IsInRecovery => IsRecovery || IsRecoveryFromStartup;

        internal bool AuditMenuOperationValid => IsChooser || (IsGame && !IsGameLoading);
        internal bool BalanceOperationValid => (IsGame && !IsGameLoading);
        internal bool CashoutOperationValid => IsChooser || (IsGame && !IsGameLoading && (IsIdle || IsPresentationIdle));
        internal bool GameOperationValid => IsChooser || (IsGame && !IsGameLoading);
    }
}
