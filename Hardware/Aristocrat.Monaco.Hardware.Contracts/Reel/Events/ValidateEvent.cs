namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using Kernel;
    using ProtoBuf;
    using static System.FormattableString;

    /// <summary>Definition of the ValidateEvent class.</summary>
    [ProtoContract]
    public class ValidateEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidateEvent" /> class.
        /// </summary>
        /// <param name="propertyName">The property name to which the validate event applies</param>
        /// <param name="valid">Indicates whether valid or not</param>

        public ValidateEvent(string propertyName, bool valid)
        {
            PropertyName = propertyName;
            Valid = valid;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing
        /// </summary>
        public ValidateEvent()
        { }

        /// <summary>
        ///     Gets the property name to which the validate event applies
        /// </summary>
        [ProtoMember(1)]
        public string PropertyName { get; }

        /// <summary>
        ///     Gets whether valid or not
        /// </summary>
        [ProtoMember(2)]
        public bool Valid { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [PropertyName={PropertyName}] [Valid={Valid}]");
        }
    }
}