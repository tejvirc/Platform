namespace Aristocrat.Monaco.Application.Contracts.AlteredMediaLogger
{
    using System.Collections.Generic;

    /// <summary>
    ///     An interface to provide the access to the AlteredMedia log.
    /// </summary>
    /// <para>
    ///     Through this interface, the already logged messages can be retrieved
    ///     and a whenever any altered media get changed a log will be entered in persistence storage.
    /// </para>
    /// <para>
    ///     The component implementing this interface must save the log messages
    ///     so that other components can retrieve them for display or for other
    ///     purposes. If the amount of messages exceeds the maximum, it should
    ///     retain the most recent ones.
    /// </para>
    public interface IAlteredMediaLogger
    {
        /// <summary>
        ///     Gets the list of message stored in persistence storage
        /// </summary>
        IEnumerable<AlteredMediaLogMessage> Logs { get; }
    }
}