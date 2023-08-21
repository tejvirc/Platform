namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Provides methods to indicate when we are in a cashable lockup condition
    /// </summary>
    public interface ICashableLockupProvider
    {
        /// <summary>
        ///     Returns a value indicating whether we can cash out
        ///     during a lock up or not
        /// </summary>
        /// <param name="isLockupMessageVisible">
        /// indicates whether a lock up message is visible</param>
        /// <param name="cashOutEnabled">
        /// indicates whether we have credits that can be cashed out</param>
        /// <param name="cashoutMethod">The method to call to force a cashout</param>
        /// <returns>True if we can cash out during a lock up, false otherwise</returns>
        bool CanCashoutInLockup(bool isLockupMessageVisible, bool cashOutEnabled, Action cashoutMethod);
    }
}