namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    ///     Class to hold call backs used by the HostAcknowledgementProvider.
    /// </summary>
    public class HostAcknowledgementHandler : IHostAcknowledgementHandler
    {
        /// <inheritdoc />
        public Action ImpliedAckHandler { get; set; }

        /// <inheritdoc />
        public Action ImpliedNackHandler { get; set; }
    }
}