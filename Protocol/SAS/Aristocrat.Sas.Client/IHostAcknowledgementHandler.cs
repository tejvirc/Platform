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

        /// <summary>
        ///     Gets or sets the handler method to use when we get an intermediate Nack.
        ///     Only EFT is using these to reset the timers at the moment.
        /// </summary>
        Action IntermediateNackHandler { get; set; }
    }
}