namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Drm;

    internal interface IProtectionModule : ILicenseMode, IDisposable
    {
        /// <summary>
        ///     Gets the available tokens
        /// </summary>
        IEnumerable<IToken> Tokens { get; }

        /// <summary>
        ///     Initialize the module
        /// </summary>
        /// <returns>a Task that can be awaited</returns>
        Task Initialize();

        /// <summary>
        ///     Determines if the module is connected
        /// </summary>
        /// <returns>true if connected</returns>
        bool IsConnected();

        /// <summary>
        ///     Decrements the specified counter
        /// </summary>
        /// <param name="token">The token to update</param>
        /// <param name="counter">The index of the counter to update</param>
        /// <param name="value">The value to decrement</param>
        /// <returns>true if the counter was successfully decremented</returns>
        bool DecrementCounter(IToken token, Counter counter, int value);
    }
}