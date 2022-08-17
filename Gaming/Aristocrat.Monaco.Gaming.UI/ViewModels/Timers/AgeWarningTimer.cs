namespace Aristocrat.Monaco.Gaming.UI.ViewModels.Timers
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Contracts.Lobby;
    using Contracts.Models;
    using Kernel;
    using log4net;

    public class AgeWarningTimer : IDisposable
    {
        private static readonly TimeSpan AgeWarningDialogTimeOutInSeconds = new TimeSpan(0, 0, 5);
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ILobbyStateManager _lobbyStateManager;

        private Timer _ageWarningTimer;
        
        private bool _disposed;

        public AgeWarningTimer(ILobbyStateManager stateManager)
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            _lobbyStateManager = stateManager;

            _ageWarningTimer = new Timer(AgeWarningTimerTick, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _lobbyStateManager.CheckForAgeWarning = CheckForAgeWarning;
        }

        public bool AgeWarningNeeded { get; set; }

        public void Stop()
        {
            Logger.Debug("Stopping AgeWarningDialog");

            _ageWarningTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public AgeWarningCheckResult CheckForAgeWarning()
        {
            var result = AgeWarningCheckResult.False;

            if (AgeWarningNeeded && !_lobbyStateManager.ContainsAnyState(LobbyState.CashIn))
            {
                if (_lobbyStateManager.ContainsAnyState(LobbyState.Disabled))
                {
                    // This will cause us to come out of disabled to the Age Warning Dialog
                    result = AgeWarningCheckResult.DisableDeferred;
                }
                else
                {
                    _lobbyStateManager.SendTrigger(LobbyTrigger.AgeWarningDialog);
                    result = AgeWarningCheckResult.True;
                }
            }

            Logger.Debug($"CheckForAgeWarning {result}");

            return result;
        }

        public void OnAgeWarningDialogEnter()
        {
            Logger.Debug("Entering AgeWarningDialog");

            AgeWarningNeeded = false;

            _ageWarningTimer.Change(AgeWarningDialogTimeOutInSeconds, Timeout.InfiniteTimeSpan);

            _eventBus.Publish(new AgeWarningDialogVisibleEvent());
        }

        public void OnAgeWarningDialogExit()
        {
            Logger.Debug("Exiting AgeWarningDialog");

            _eventBus.Publish(new AgeWarningDialogHiddenEvent());
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _ageWarningTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _ageWarningTimer.Dispose();
            }

            _ageWarningTimer = null;

            _disposed = true;
        }

        private void AgeWarningTimerTick(object sender)
        {
            Logger.Debug("AgeWarningDialog timed out");

            _lobbyStateManager?.SendTrigger(LobbyTrigger.AgeWarningTimeout);
        }
    }
}