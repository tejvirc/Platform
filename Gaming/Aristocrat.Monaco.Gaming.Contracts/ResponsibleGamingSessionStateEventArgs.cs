namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Enum for the current state of Responsible Gaming
    /// </summary>
    public enum ResponsibleGamingSessionState
    {
        /// <summary>
        /// Stopped State
        /// </summary>
        Stopped,

        /// <summary>
        /// Started (Running) State
        /// </summary>
        Started,

        /// <summary>
        /// Paused State
        /// </summary>
        Paused,

        /// <summary>
        /// Disabled State (Means Stopped, but disabled.  Needed so that a start will go to paused, not started
        /// </summary>
        Disabled
    }

    /// <summary>
    ///     Delegate for ResponsibleGamingStateChangeEvent
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ResponsibleGamingStateChangeEventHandler(
        object sender,
        ResponsibleGamingSessionStateEventArgs e);

    /// <summary>
    ///     Event Args
    /// </summary>
    public class ResponsibleGamingSessionStateEventArgs : EventArgs
    {
        /// <summary>
        ///     Event Args for a ResponsibleGamingSessionStateChangedEvent
        /// </summary>
        /// <param name="state"></param>
        /// <param name="sessionTimeRemaining"></param>
        public ResponsibleGamingSessionStateEventArgs(
            ResponsibleGamingSessionState state,
            TimeSpan sessionTimeRemaining)
        {
            State = state;
            SessionTimeRemaining = sessionTimeRemaining;
        }

        /// <summary>
        ///     Returns the Current State of Responsible Gaming
        /// </summary>
        public ResponsibleGamingSessionState State { get; }

        /// <summary>
        ///     Time remaining in the current Responsible Gaming session
        /// </summary>
        public TimeSpan SessionTimeRemaining { get; }
    }
}
