namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    /// <summary>
    ///     Presentation loader for bingo that needs to occur when the game is started
    /// </summary>
    public interface IBingoPresentationLoader
    {
        /// <summary>
        ///     Loads the presentation
        /// </summary>
        public void LoadPresentation();
    }
}