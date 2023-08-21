namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor SetValidationEvent class.</summary>
    /// <remarks>
    ///     The SetValidationEvent is posted by the IdReaderService if the IdReader is disabled
    ///     or an attempt to enable a disabled note acceptor failed.
    /// </remarks>
    [Serializable]
    public class SetValidationEvent : IdReaderBaseEvent
    {
        /// <summary>Initializes a new instance of the <see cref="SetValidationEvent"/> class.</summary>
        public SetValidationEvent(Identity identity)
        {
            Identity = identity;
        }

        /// <summary>Initializes a new instance of the <see cref="SetValidationEvent"/> class.</summary>
        /// <param name="idReaderId">Identifier for the ID reader.</param>
        /// <param name="identity">The identity.</param>
        public SetValidationEvent(int idReaderId, Identity identity)
            : base(idReaderId)
        {
            Identity = identity;
        }

        /// <summary>Gets the identity.</summary>
        /// <value>The identity.</value>
        public Identity Identity { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Identity={Identity}]");
        }
    }
}
