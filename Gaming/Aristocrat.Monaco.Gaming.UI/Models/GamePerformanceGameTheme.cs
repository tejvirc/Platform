namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Toolkit.Mvvm.Extensions;

    /// <summary>
    ///     Data for filtered Game Themes.
    /// </summary>
    public class GamePerformanceGameTheme : BaseObservableObject
    {
        private bool _checked;

        /// <summary>
        ///     Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the game type.
        /// </summary>
        public string GameType { get; set; }

        /// <summary>
        ///     Gets or sets the game type.
        /// </summary>
        public string GameSubtype { get; set; }

        /// <summary>
        ///     Gets or sets whether to filter out this Game Theme.
        /// </summary>
        public bool Checked
        {
            get => _checked;
            set => SetProperty(ref _checked, value, nameof(Checked));
        }

        public int GameId { get; set; }
    }
}
