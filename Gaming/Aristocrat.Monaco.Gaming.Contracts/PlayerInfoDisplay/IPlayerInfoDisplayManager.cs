namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Responsible for: Showing/Hiding Main PID Page, Switching between the sub-pages, Communicating with Runtime for the sub-pages/sub-feature supported by Runtime
    /// </summary>
    public interface IPlayerInfoDisplayManager : IDisposable
    {
        /// <summary>
        ///     Indicates Play Information Display is active/inactive
        /// </summary>
        /// <returns></returns>
        bool IsActive();

        /// <summary>
        /// Setup Play Information Display screens implemented by  Platform
        /// </summary>
        void AddPages(IList<IPlayerInfoDisplayViewModel> pages);
    }
}