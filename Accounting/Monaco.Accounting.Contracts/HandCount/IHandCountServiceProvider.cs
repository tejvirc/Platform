namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     Contract for hand count instance.
    /// </summary>
    public interface IHandCountServiceProvider : IService, IDisposable
    {
        /// <summary>
        /// Return hand count
        /// </summary>
        bool HandCountServiceEnabled { get; }

        /// <summary>
        /// Return hand count
        /// </summary>
        int HandCount { get; }

        /// <summary>
        ///     Increment hand count
        /// </summary>
        void IncrementHandCount();

        /// <summary>
        /// Reduce hand counts by the number provided in param
        /// </summary>
        /// <param name="number"></param>
        void DecreaseHandCount(int number);

        /// <summary>
        /// Check and run if reset hand count is required
        /// </summary>
        void CheckAndResetHandCount();
    }
}
