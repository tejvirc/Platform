namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System;

    /// <summary>Definition of the <c>KeySwitchOnEvent</c> class.</summary>
    [Serializable]
    public class OnEvent : KeySwitchBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OnEvent" /> class.
        /// </summary>
        public OnEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OnEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical key switch ID.</param>
        /// <param name="keySwitchName">The name of the key switch.</param>
        public OnEvent(int logicalId, string keySwitchName)
            : base(logicalId, keySwitchName)
        {
        }
    }
}