namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    /// <summary>Valid key switch state enumerations.</summary>
    public enum KeySwitchState
    {
        /// <summary>Indicates key switch state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates key switch state disabled.</summary>
        Disabled,

        /// <summary>Indicates key switch state enabled.</summary>
        Enabled,

        /// <summary>Indicates key switch state error.</summary>
        Error
    }

    /// <summary>Valid key switch action enumerations.</summary>
    public enum KeySwitchAction
    {
        /// <summary>Indicates key switch action on.</summary>
        On = 0,

        /// <summary>Indicates key switch action off.</summary>
        Off
    }

    /// <summary>Class to be used in a generic List for management of logical key switches.</summary>
    public class LogicalKeySwitch : LogicalDeviceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalKeySwitch" /> class.
        /// </summary>
        public LogicalKeySwitch()
        {
            State = KeySwitchState.Uninitialized;
            Action = KeySwitchAction.On;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalKeySwitch" /> class.
        /// </summary>
        /// <param name="physicalId">The physical key switch ID.</param>
        /// <param name="name">The key switch name.</param>
        /// <param name="localizedName">The localized key switch name.</param>
        public LogicalKeySwitch(
            int physicalId,
            string name,
            string localizedName)
            : base(physicalId, name, localizedName)
        {
            State = KeySwitchState.Uninitialized;
            Action = KeySwitchAction.Off;
        }

        /// <summary>Gets or sets a value for key switch state.</summary>
        public KeySwitchState State { get; set; }

        /// <summary>Gets or sets a value for key switch action.</summary>
        public KeySwitchAction Action { get; set; }
    }
}