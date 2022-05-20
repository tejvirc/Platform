namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System;

    /// <summary>Definition of the <c>KeyHeldEvent</c> class.</summary>
    [Serializable]
    public class KeyHeldEvent : KeySwitchBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyHeldEvent" /> class.
        /// </summary>
        public KeyHeldEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyHeldEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical key switch ID.</param>
        /// <param name="keySwitchName">The name of the key switch.</param>
        public KeyHeldEvent(int logicalId, string keySwitchName)
            : base(logicalId, keySwitchName)
        {
        }
    }
}