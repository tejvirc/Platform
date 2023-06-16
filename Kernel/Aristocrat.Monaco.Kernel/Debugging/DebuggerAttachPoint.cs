namespace Aristocrat.Monaco.Kernel.Debugging
{
    /// <summary>
    /// Point in application to auto launch/attach the debugger.
    /// </summary>
    public enum DebuggerAttachPoint
    {
        /// <summary>
        /// No attach point defined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Attach debugger upon entering the initial configuration check. Useful for debugging the Configuration Wizard.
        /// </summary>
        OnInitialConfigurationCheck,

        /// <summary>
        /// Attach debugger after system initialization completes.
        /// </summary>
        OnSystemInitialized,
    }
}
