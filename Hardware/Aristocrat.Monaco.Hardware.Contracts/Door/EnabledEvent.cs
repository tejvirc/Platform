namespace Aristocrat.Monaco.Hardware.Contracts.Door
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;
    using SharedDevice;

    /// <summary>Definition of the EnabledEvent class.</summary>
    [ProtoContract]
    public class EnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        [CLSCompliant(false)]
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public EnabledEvent()
        { }

        /// <summary>Gets the reasons for the enabled event.</summary>
        [CLSCompliant(false)]
        [ProtoMember(1)]
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Door {0} {1}",
                GetType().Name,
                Reasons);
        }
    }
}