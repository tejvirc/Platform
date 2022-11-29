namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System;
    using Kernel;

    /// <summary>
    ///     The GameLanguageChangedEvent is posted when the player changes the language in the game.
    /// </summary>
    [Serializable]
    public class GameLanguageChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameLanguageChangedEvent" /> class.
        /// </summary>
        /// <param name="localeCode">Locale selected by player</param>
        public GameLanguageChangedEvent(string localeCode)
        {
            LocaleCode = localeCode;
        }

        /// <summary>
        ///     Locale code selected by player
        /// </summary>
        public string LocaleCode { get; }
    }
}
