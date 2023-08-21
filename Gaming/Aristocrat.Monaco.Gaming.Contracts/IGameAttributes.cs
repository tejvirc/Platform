namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    ///     Provides a mechanism to get attributes about the game.
    /// </summary>
    public interface IGameAttributes
    {
        /// <summary>
        ///     Gets the folder of the game.
        /// </summary>
        string Folder { get; }

        /// <summary>
        ///     Gets the name of the game DLL.
        /// </summary>
        string GameDll { get; }

        /// <summary>
        ///     Gets the locale graphics for the game.
        /// </summary>
        Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; }

        /// <summary>
        ///     Gets the meter name used to retrieve and display the current bonus or progressive value in the lobby or anywhere in
        ///     the UI.
        /// </summary>
        string DisplayMeterName { get; }

        /// <summary>
        ///     Gets the meter names used to retrieve and display the current progressive values for associated SAP levels
        /// </summary>
        IEnumerable<string> AssociatedSapDisplayMeterName { get; }

        /// <summary>
        ///     Gets the game icon type to use for the progressive label
        /// </summary>
        GameIconType GameIconType { get; }

        /// <summary>
        ///     Gets the initial value for a bonus or progressive game, which is used in the lobby or anywhere in the UI.
        ///     Typically only used until there has been at least one game round, since the game meter values trump this value.
        /// </summary>
        long InitialValue { get; }

        /// <summary>
        ///     Gets the target runtime
        /// </summary>
        string TargetRuntime { get; }
    }
}
