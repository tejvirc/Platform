namespace Aristocrat.Monaco.Gaming.UI.PlayerInfoDisplay
{
    using System.Collections.Generic;
    using Contracts.PlayerInfoDisplay;

    /// <summary>
    ///     Implements empty IPlayerInfoDisplayManager which is always inactive
    /// </summary>
    public sealed class PlayerInfoDisplayNotSupportedManager : IPlayerInfoDisplayManager
    {
        /// <inheritdoc />
        public void Dispose()
        {
            // nothing to dispose
        }

        /// <inheritdoc />
        public bool IsActive()
        {
            return false;
        }

        /// <inheritdoc />
        public void AddPages(IList<IPlayerInfoDisplayViewModel> pages)
        {
            // ignore any screens
        }
    }
}