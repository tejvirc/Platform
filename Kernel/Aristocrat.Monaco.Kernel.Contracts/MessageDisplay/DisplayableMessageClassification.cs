namespace Aristocrat.Monaco.Kernel.Contracts.MessageDisplay
{
    /// <summary>
    ///     This enumeration is used to classify messages so that handlers can decide how to present them.
    /// </summary>
    /// <remarks>
    ///     This is currently used as a property in <see cref="IDisplayableMessage" /> objects.
    /// </remarks>
    public enum DisplayableMessageClassification
    {
        /// <summary>
        ///     The message is regarding a serious error that will likely require a
        ///     hard-reboot of the system.
        /// </summary>
        HardError,

        /// <summary>
        ///     The message is regarding an error that can likely be remedied
        ///     without a hard-reboot of the system.
        /// </summary>
        SoftError,

        /// <summary>
        ///     The message contains important information for the user.
        /// </summary>
        Informative,

        /// <summary>
        ///     The message contains information useful for diagnostics
        /// </summary>
        Diagnostic
    }
}