namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     An implementation of an <see cref="IWinLevel" />/>
    /// </summary>
    public class WinLevel : IWinLevel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WinLevel" /> class.
        /// </summary>
        /// <param name="winLevelIndex">Unique paytable index for the win level</param>
        /// <param name="winLevelCombo">Description of the paytable win level</param>
        /// <param name="progressiveAllowed">Indicates whether the paytable win level is permitted to be assigned to a progressive</param>
        public WinLevel(int winLevelIndex, string winLevelCombo, bool progressiveAllowed)
        {
            WinLevelIndex = winLevelIndex;
            WinLevelCombo = winLevelCombo;
            ProgressiveAllowed = progressiveAllowed;
        }

        /// <inheritdoc />
        public int WinLevelIndex { get; }

        /// <inheritdoc />
        public string WinLevelCombo { get; }

        /// <inheritdoc />
        public bool ProgressiveAllowed { get; }
    }
}