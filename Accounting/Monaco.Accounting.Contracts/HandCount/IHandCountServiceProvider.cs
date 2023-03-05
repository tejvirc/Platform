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
    public interface IHandCountServiceProvider : IService
    {
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
    }
}
