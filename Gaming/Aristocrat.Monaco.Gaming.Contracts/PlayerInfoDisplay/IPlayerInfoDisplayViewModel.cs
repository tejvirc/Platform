namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    using System;

    /// <summary>
    ///     Player Information Display Screen
    /// </summary>
    public interface IPlayerInfoDisplayViewModel
    {
        /// <summary>
        ///     Indicate what type of Player Information Display screen
        /// </summary>
        PageType PageType { get; }

        /// <summary>
        ///     Show Player Information Display screen
        /// </summary>
        void Show();

        /// <summary>
        ///     Hide Player Information Display screen
        /// </summary>
        void Hide();

        /// <summary>
        ///     Fires when Show Player Information Display screen button clicked
        /// </summary>
        event EventHandler<CommandArgs> ButtonClicked;

        /// <summary>
        ///     Set resources
        /// </summary>
        /// <param name="resourcesModel"></param>
        void SetupResources(IPlayInfoDisplayResourcesModel resourcesModel);
    }
}