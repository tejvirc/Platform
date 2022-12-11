namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;
    using SharedDevice;

    /// <summary>Definition of the DisabledEvent class.</summary>
    [ProtoContract]
    public class DisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        [CLSCompliant(false)]
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public DisabledEvent()
        { }

        /// <summary>Gets the reasons for the disabled event.</summary>
        [CLSCompliant(false)]

        [ProtoMember(1)]
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Button {0} {1}",
                GetType().Name,
                Reasons);
        }
    }
}