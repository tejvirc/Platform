namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    /// <summary>Valid button action enumerations.</summary>
    public enum TouchState
    {
        /// <summary>Indicates button action up.</summary>
        None = -1,

        /// <summary>Indicates button action up.</summary>
        Up = 0,

        /// <summary>Indicates button action down.</summary>
        Down = 1
    }

    /// <summary>Valid button action enumerations.</summary>
    public enum TouchAction
    {
        /// <summary>Indicates no action.</summary>
        None = 0,

        /// <summary>Indicates left mouse button action.</summary>
        LeftMouse = 1,

        /// <summary>Indicates touch action.</summary>
        Touch = 1,

        /// <summary>Indicates middle mouse button action.</summary>
        MiddleMouse = 2,

        /// <summary>Indicates right mouse button action.</summary>
        RightMouse = 4,

        /// <summary>Indicates touch/mouse move.</summary>
        Move = 8,

        /// <summary>Indicates wheel/scroll action.</summary>
        Scroll = 16
    }

    /// <summary>Class contains Touch action data.</summary>
    public class TouchInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchInfo" /> class.
        /// </summary>
        public TouchInfo()
        {
            TouchId = 0;
            X = 0;
            Y = 0;
            State = TouchState.None;
            Action = TouchAction.None;
        }

        /// <summary>Gets or sets a value for X.</summary>
        public int X { get; set; }

        /// <summary>Gets or sets a value for Y.</summary>
        public int Y { get; set; }

        /// <summary>Gets or sets a value for TouchID. MultiTouch events has unique id's for each touch.</summary>
        public int TouchId { get; set; }

        /// <summary>Gets or sets a value for Y.</summary>
        public TouchState State { get; set; }

        /// <summary>Gets or sets a value for Y.</summary>
        public TouchAction Action { get; set; }

        /// <summary>Gets or sets a value for scroll/wheel.</summary>
        public int Scroll { get; set; }
    }
}