namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     This enumeration is used to control when a handler displays a message.
    /// </summary>
    /// <remarks>
    ///     This is currently used as a property in <see cref="DisplayableMessage" /> objects.
    /// </remarks>
    public enum DisplayableMessagePriority
    {
        /// <summary>
        ///     The message needs to be displayed immediately.
        /// </summary>
        Immediate,

        /// <summary>
        ///     The message need not interrupt anything, but should be displayed
        ///     as soon as is appropriate.
        /// </summary>
        Normal,

        /// <summary>
        ///     The message can be displayed when appropriate, potentially after
        ///     higher priority messages have been cleared.
        /// </summary>
        Low
    }
}