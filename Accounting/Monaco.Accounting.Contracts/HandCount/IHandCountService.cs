﻿namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;


    /// <summary>
    ///     Contract for hand count service instance.
    /// </summary>
    public interface IHandCountService : IService
    {
        /// <summary>
        ///     Gets hand count
        /// </summary>
        /// <returns>GameCategorySetting</returns>
        int HandCount { get; }

        /// <summary>
        ///     Increment hand count
        /// </summary>
        void IncrementHandCount();

        /// <summary>
        ///     Decrease hand count
        /// <param name="n">Decrease hand count by n.</param>
        /// </summary>
        void DecreaseHandCount(int n);
        /// <summary>
        ///     Reset hand count
        /// </summary>
        void ResetHandCount();

        /// <summary>
        ///     Send HandCountChangedEvent
        /// </summary>
        void SendHandCountChangedEvent();

       
    }
}