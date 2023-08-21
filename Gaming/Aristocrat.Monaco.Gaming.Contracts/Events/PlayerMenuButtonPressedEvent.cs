namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Kernel;

    /// <summary>
    /// Event for the pressing of the Player Menu button
    /// </summary>
    public class PlayerMenuButtonPressedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerMenuButtonPressedEvent"/> class.
        /// </summary>
        /// <param name="show">True means the button was pressed to open the menu, false to close it</param>
        public PlayerMenuButtonPressedEvent(bool show)
        {
            Show = show;
        }

        /// <summary>
        ///     True means the button was pressed to open the menu, false to close it
        /// </summary>
        public bool Show { get; set; }
    }
}
