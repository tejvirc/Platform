namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Valid note acceptor jam location enumerations.</summary>
    public enum JamLocation
    {
        /// <summary>Indicates jam location unknown.</summary>
        Unknown,

        /// <summary>Indicates jam location is in the acceptor.</summary>
        Acceptor,

        /// <summary>Indicates jam location is in the stacker.</summary>
        Stacker
    }

    /// <summary>Definition of the JamEvent class.</summary>
    /// <remarks>
    ///     This is an event that is posted by a note acceptor component
    ///     when there is a jam in the document path.
    /// </remarks>
    [Serializable]
    public class JamEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JamEvent" /> class.
        /// </summary>
        public JamEvent()
        {
            Location = JamLocation.Unknown;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JamEvent" /> class.
        /// </summary>
        /// <param name="location">The type of note acceptor jam. </param>
        public JamEvent(JamLocation location)
        {
            Location = location;
        }

        /// <summary>Gets the note acceptor jam location.</summary>
        public JamLocation Location { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Location={1}]",
                GetType().Name,
                Location);
        }
    }
}