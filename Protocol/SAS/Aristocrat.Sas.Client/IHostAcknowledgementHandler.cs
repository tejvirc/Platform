namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    ///     Allows passing handler information to the HostAcknowledgementProvider
    /// </summary>
    public interface IHostAcknowledgementHandler
    {
        /// <summary>
        ///     Gets or sets the handler method to use when we get an implied Ack
        /// </summary>
        Action ImpliedAckHandler { get; set; }

        /// <summary>
        ///     Gets or sets the handler method to use when we get an implied Nack
        /// </summary>
        Action ImpliedNackHandler { get; set; }
    }
}