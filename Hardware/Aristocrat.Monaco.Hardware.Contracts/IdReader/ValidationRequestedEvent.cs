namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using CardReader;
    using static System.FormattableString;

    /// <summary>Definition of the ID reader ValidationRequestedEvent class.</summary>
    /// <remarks>
    ///     The ValidationRequestedEvent is posted by the IdReaderAdapter if the ID reader needs an ID number validated.
    /// </remarks>
    [Serializable]
    public class ValidationRequestedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationRequestedEvent" /> class.
        /// </summary>
        /// <param name="idReaderId">Identifier for the ID reader.</param>
        /// <param name="trackData">Track data read from the magnetic stripe.</param>
        public ValidationRequestedEvent(int idReaderId, TrackData trackData)
            : base(idReaderId)
        {
            TrackData = trackData;
        }

        /// <summary>Gets the track data.</summary>
        public TrackData TrackData { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [IdNumber={TrackData?.IdNumber}]");
        }
    }
}
