namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;
    using System;


    /// <summary>
    ///     Interface for a hand count service, which tracks the number of games a player has played.
    /// </summary>
    public interface IHandCountService : IService, IDisposable
    {
        /// <summary>
        ///     Returns true if the HandCountService is required for this jurisdiction.
        /// </summary>
        bool HandCountServiceEnabled { get; }

        /// <summary>
        ///     Gets hand count, which is the number of games a player has played. When cashing out,
        ///     the player must have enough hand count and it will be consumed by cashing out.
        /// </summary>
        int HandCount { get; }

        /// <summary>
        ///     Increment hand count
        /// </summary>
        void IncrementHandCount();

        /// <summary>
        ///     Decrease hand count
        /// </summary>
        /// <param name="n">Amount to decrease hand count</param>
        void DecreaseHandCount(int n);

        /// <summary>
        ///     Check if reset is required because we are below the minimum amount of credits
        /// </summary>
        void CheckIfBelowResetThreshold();
    }
}