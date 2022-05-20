namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    /// <summary>
    ///     This creates the message that will be queued and sent to SAS
    /// </summary>
    public interface ISasMessage
    {
        /// <summary>
        ///     The message data to be sent to SAS
        /// </summary>
        IReadOnlyCollection<byte> MessageData { get; }
    }
}