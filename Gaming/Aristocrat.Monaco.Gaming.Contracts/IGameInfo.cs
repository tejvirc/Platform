using System;

namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    /// Game info
    /// </summary>
    public interface IGameInfo
    {
        /// <summary>
        ///     Theme ID of the game.  Not currently displayed anywhere so doesn't need OnPropertyChanged
        /// </summary>
        string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the date created
        /// </summary>
        DateTime InstallDateTime { get; set; }
    }
}
