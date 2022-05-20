namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     The KeySwitchEvent is fired when a key switch is turned
    /// </summary>
    [Serializable]
    public abstract class KeySwitchEvent : BaseEvent
    {
        /// <summary>
        ///     The switch id
        /// </summary>
        private readonly int _keySwitchId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeySwitchEvent" /> class.
        /// </summary>
        protected KeySwitchEvent()
        {
            // Used by KeyToEventConverter
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeySwitchEvent" /> class.
        /// </summary>
        /// <param name="keySwitchId">The id of the key switch</param>
        protected KeySwitchEvent(int keySwitchId)
        {
            _keySwitchId = keySwitchId;
        }

        /// <summary>
        ///     Gets the id of the switch
        /// </summary>
        public int SwitchId => _keySwitchId;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [KeySwitchId={1}]",
                GetType().Name,
                _keySwitchId);
        }
    }
}