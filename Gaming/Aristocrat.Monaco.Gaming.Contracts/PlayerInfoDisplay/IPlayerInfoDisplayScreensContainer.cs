namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Container of Player Information Display Screens
    /// </summary>
    public interface IPlayerInfoDisplayScreensContainer
    {

        /// <summary>
        ///     List of hosted Player Information Display Screens
        /// </summary>
        IList<IPlayerInfoDisplayViewModel> AvailablePages { get; }
    }
}