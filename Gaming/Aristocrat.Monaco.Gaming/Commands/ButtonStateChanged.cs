namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    /// <summary>
    ///     Button state command
    /// </summary>
    public class ButtonStateChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ButtonStateChanged" /> class.
        /// </summary>
        /// <param name="newStates">The button/state map</param>
        public ButtonStateChanged(IDictionary<uint, ButtonState> newStates)
        {
            States = newStates;
        }

        /// <summary>
        ///     Gets the button mask
        /// </summary>
        public IDictionary<uint, ButtonState> States { get; }
    }
}