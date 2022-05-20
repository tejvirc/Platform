namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    /// The current mode of the machine as defined by ASP protocol.
    /// Represents the result sent back by C2T2P1.
    /// Note that only one bit can be set at a time.
    /// For Asp1000/2000 bits 1-6 are reserved.
    /// For Asp5000/Dacom bits 1-4 and 7-9 are reserved.
    /// </summary>
    public enum MachineMode
    {
        /// <summary>
        /// Bit 0 - The EGM has locked up, waiting for reboot.
        /// </summary>
        FatalError = 1,

        /// <summary>
        /// Bit 5 - A game is being replayed via the operator menu.
        /// </summary>
        GameReplayActive = 32,

        /// <summary>
        /// Bit 6 - EGM manually set to out-of-service by operator via operator menu.
        /// </summary>
        EgmOutOfService = 64,

        /// <summary>
        /// Bit 10 - EGM ready to play a game and not in locked-up state.
        /// </summary>
        Idle = 1024,

        /// <summary>
        /// Bit 11 - Operator menu is showing.
        /// </summary>
        AuditMode = 2048,

        /// <summary>
        /// Bit 12 - Demonstration mode active.
        /// Demonstration mode is not implemented on Linux or Windows.
        /// </summary>
        DemoMode = 4096,

        /// <summary>
        /// Bit 13 - A diagnostic test is running.
        /// </summary>
        DiagnosticTest = 8192,

        /// <summary>
        /// Bit 14 - A game is in progress.
        /// </summary>
        GameInProgress = 16384,

        /// <summary>
        /// Bit 15 - The EGM has powered-up and is setting itself up.
        /// </summary>
        NotInOperation = 32768,
    }
}