namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;

    /// <summary>Class to handle key switch events.</summary>
    [ProtoContract]
    public abstract class KeySwitchBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="KeySwitchBaseEvent" /> class.
        /// </summary>
        protected KeySwitchBaseEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeySwitchBaseEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical ID of the key switch.</param>
        /// <param name="keySwitchName">The name of the key switch.</param>
        protected KeySwitchBaseEvent(int logicalId, string keySwitchName)
        {
            LogicalId = logicalId;
            KeySwitchName = keySwitchName;
        }

        /// <summary>Gets a value indicating whether LogicalId is set.</summary>
        [ProtoMember(1)]
        public int LogicalId { get; }

        /// <summary>Gets the value of the key switch name.</summary>
        [ProtoMember(2)]
        public string KeySwitchName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}",
                KeySwitchName,
                GetType().Name);
        }
    }
}