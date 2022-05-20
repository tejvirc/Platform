namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to retrieve general user activity
    /// </summary>
    public interface IUserActivityService : IService
    {
        /// <summary>
        ///     Gets the last known user action
        /// </summary>
        /// <returns></returns>
        DateTime? GetLastAction();
    }
}
