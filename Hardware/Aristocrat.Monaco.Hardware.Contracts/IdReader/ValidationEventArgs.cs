namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using CardReader;

    /// <summary>Additional information for validation events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class ValidationEventArgs : EventArgs
    {
        /// <summary>Gets or sets the track data</summary>
        public TrackData TrackData { get; set; }
    }
}
