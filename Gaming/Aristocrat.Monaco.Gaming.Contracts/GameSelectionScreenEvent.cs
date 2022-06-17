namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using System;
    using System.Globalization;

    /// <summary>
    ///     An event that is posted when the GDK indicates that it has entered or exited an in-game
    ///     "selection screen", for example a denomination lobby.
    /// </summary>
    [Serializable]
    public class GameSelectionScreenEvent : BaseEvent
    {
        /// <summary>
        ///     Indicates whether we are entering or exiting the in-game selection screen.
        /// </summary>
        public bool IsEntering;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectionScreenEvent"/> class.
        /// </summary>
        /// <param name="isEntering">True for entering the screen, false for exiting</param>
        public GameSelectionScreenEvent(bool isEntering)
        {
            IsEntering = isEntering;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, $"{GetType().Name} Entering={IsEntering}");
        }
    }
}