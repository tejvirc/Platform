namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    /// <summary>Valid button state enumerations.</summary>
    public enum ButtonState
    {
        /// <summary>Indicates button state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates button state disabled.</summary>
        Disabled,

        /// <summary>Indicates button state enabled.</summary>
        Enabled,

        /// <summary>Indicates button state error.</summary>
        Error
    }

    /// <summary>Valid button action enumerations.</summary>
    public enum ButtonAction
    {
        /// <summary>Indicates button action up.</summary>
        Up = 0,

        /// <summary>Indicates button action down.</summary>
        Down
    }

    /// <summary>Class to be used in a generic List for management of logical buttons.</summary>
    public class LogicalButton : LogicalDeviceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalButton" /> class.
        /// </summary>
        public LogicalButton()
        {
            State = ButtonState.Uninitialized;
            Action = ButtonAction.Up;
            LampBit = -1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalButton" /> class.
        /// </summary>
        /// <param name="physicalId">The physical button ID.</param>
        /// <param name="name">The button name.</param>
        /// <param name="localizedName">The localized button name.</param>
        /// <param name="lampBit">The lamp bit.</param>
        public LogicalButton(int physicalId, string name, string localizedName, int lampBit)
            : base(physicalId, name, localizedName)
        {
            State = ButtonState.Uninitialized;
            Action = ButtonAction.Up;
            LampBit = lampBit;
        }

        /// <summary>Gets or sets a value for button state.</summary>
        public ButtonState State { get; set; }

        /// <summary>Gets or sets a value for button action.</summary>
        public ButtonAction Action { get; set; }

        /// <summary>Gets or sets a value for button lamp bit.</summary>
        public int LampBit { get; set; }
    }
}