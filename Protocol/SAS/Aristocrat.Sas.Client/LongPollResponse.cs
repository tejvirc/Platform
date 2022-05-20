namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    /// Common base class that all the long poll responses implement
    /// </summary>
    [Serializable]
    public class LongPollResponse
    {
        /// <summary>Gets or sets the HostAcknowledgementHandler handlers.</summary>
        public IHostAcknowledgementHandler Handlers { get; set; }
    }
}