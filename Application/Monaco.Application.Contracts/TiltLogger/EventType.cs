namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System;

    /// <summary>
    ///     Definition of the EventType class.
    /// </summary>
    public partial class EventType : IEquatable<EventType>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventType" /> class.  To empty, not null.
        /// </summary>
        public EventType()
            : this(string.Empty, 0, string.Empty)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventType" /> class.
        /// </summary>
        /// <param name="type">The parameter is the type of event.</param>
        /// <param name="max">The parameter is the maximum number of events of this type.</param>
        /// <param name="combined">The parameter indicates a combined minimum number of events for this type.</param>
        public EventType(string type, int max, string combined)
        {
            Type = type;
            Max = max;
            Combined = combined;
        }

        /// <summary>
        ///     An indexer of properties based on the name of the property.
        /// </summary>
        /// <param name="indexer">Name of property to fetch.</param>
        /// <returns>The property value as an object.</returns>
        public object this[string indexer]
        {
            get
            {
                if (indexer == "Type")
                {
                    return Type;
                }

                if (indexer == "Max")
                {
                    return Max;
                }

                if (indexer == "Combined")
                {
                    return Combined;
                }

                return null;
            }
        }

        /// <inheritdoc />
        public bool Equals(EventType other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Type == Type && other.Max == Max && other.Combined == Combined;
        }
    }
}