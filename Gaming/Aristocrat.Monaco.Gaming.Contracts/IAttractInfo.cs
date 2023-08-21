namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.ComponentModel;
    using Models;

    /// <summary>
    /// Attract info
    /// </summary>
    public interface IAttractInfo : ICloneable, INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the game theme ID
        /// </summary>
        string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the GameType
        /// </summary>
        GameType GameType { get; }

        /// <summary>
        ///     Gets or sets the sequence number in the configured order
        /// </summary>
        int SequenceNumber { get; set; }

        /// <summary>
        ///     Theme display text for display on configuration screen.
        /// </summary>
        string ThemeNameDisplayText { get; set; }

        /// <summary>
        ///     Gets or sets if game is part of attract sequence or not.
        /// </summary>
        bool IsSelected { get; set; }
    }
}
