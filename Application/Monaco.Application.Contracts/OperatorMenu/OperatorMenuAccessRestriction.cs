namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    /// OperatorMenuAccessRestriction
    /// </summary>
    public enum OperatorMenuAccessRestriction
    {
        /// <summary></summary>
        None,
        /// <summary></summary>
        MainDoor,
        /// <summary></summary>
        MainOpticDoor,
        /// <summary></summary>
        LogicDoor,
        /// <summary></summary>
        CashBoxDoor,
        /// <summary></summary>
        JackpotKey,
        /// <summary></summary>
        ZeroCredits,
        /// <summary></summary>
        GameLoaded,
        /// <summary></summary>
        InGameRound,
        /// <summary></summary>
        GamesPlayed,
        /// <summary> </summary>
        EKeyVerified,
        /// <summary> </summary>
        InitialGameConfigurationComplete,
        /// <summary></summary>
        InitialGameConfigNotCompleteOrEKeyVerified,
        /// <summary></summary>
        ReadOnly,
        /// <summary></summary>
        NoHardLockups,
        /// <summary></summary>
        HostTechnician,
        /// <summary></summary>
        CommsOffline,
        /// <summary>Card Reader is connected but disabled</summary>
        CardReaderDisabled,
        /// <summary></summary>
        RuleSet,
        /// <summary>  </summary>
        ProgInit
    }
}
