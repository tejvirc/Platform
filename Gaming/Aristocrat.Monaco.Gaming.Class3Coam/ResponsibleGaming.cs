namespace Aristocrat.Monaco.Gaming.Class3Coam
{
    using System;
    using System.ComponentModel;
    using Contracts;

    public class ResponsibleGaming : IResponsibleGaming
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event ResponsibleGamingStateChangeEventHandler OnStateChange;

        public event EventHandler<EventArgs> OnForcePendingCheck;

        public event EventHandler ForceCashOut;

        public int SessionLimit { get; } = 0;

        public bool Enabled { get; } = false;

        public bool HasSessionLimits { get; } = false;

        public int SessionCount { get; } = 0;

        public bool IsSessionLimitHit { get; } = false;

        public bool IsTimeLimitDialogVisible { get; } = false;

        public ResponsibleGamingDialogState TimeLimitDialogState { get; } = ResponsibleGamingDialogState.Initial;

        public string ResponsibleGamingDialogResourceKey { get; } = string.Empty;

        public bool ShowTimeLimitDlgPending { get; } = false;

        public ResponsibleGamingMode ResponsibleGamingMode { get; } = ResponsibleGamingMode.Segmented;
         
        public TimeSpan RemainingSessionTime { get; } = TimeSpan.Zero;

        public bool SpinGuard { get; } = false;

        public void Initialize()
        {
        }

        public void ShowDialog(bool allowDialogWhileDisabled)
        {
        }

        public void LoadPropertiesFromPersistentStorage()
        {
        }

        public void AcceptTimeLimit(int timeLimitIndex)
        {
        }

        public void OnInitialCurrencyIn()
        {
        }

        public void EndResponsibleGamingSession()
        {
        }

        public void OnGamePlayDisabled()
        {
        }

        public void OnGamePlayEnabled()
        {
        }

        public void ResetDialog(bool resetDueToOperatorMenu)
        {
        }

        public bool CanSpinReels()
        {
            return true;
        }

        public void EngageSpinGuard()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnOnForcePendingCheck()
        {
            OnForcePendingCheck?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOnStateChange(ResponsibleGamingSessionStateEventArgs e)
        {
            OnStateChange?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnForceCashOut()
        {
            ForceCashOut?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
