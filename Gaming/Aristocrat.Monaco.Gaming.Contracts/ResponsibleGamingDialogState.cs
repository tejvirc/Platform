namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     States for the Responsible Gaming Dialog
    /// </summary>
    public enum ResponsibleGamingDialogState
    {
        /// <summary>
        ///     Initial state where you can choose multiple times for your session
        /// </summary>
        Initial,

        /// <summary>
        ///     State where you can choose multiple times for your session
        /// </summary>
        ChooseTime,

        /// <summary>
        ///     Forced cashout dialog state
        /// </summary>
        ForceCashOut,

        /// <summary>
        ///     Standard state where you receive an update on how long you have been playing
        /// </summary>
        TimeInfo,

        /// <summary>
        ///     Last warning dialog before forced cashout
        /// </summary>
        TimeInfoLastWarning,

        /// <summary>
        ///     Last warning dialog before forced cashout
        /// </summary>
        SeeionEndForceCashOut,

        /// <summary>
        ///     Play Break 1 dialog state
        /// </summary>
        PlayBreak1,

        /// <summary>
        ///     Play Break 2 dialog state
        /// </summary>
        PlayBreak2
    }
}
