namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     Contract for hand count instance.
    /// </summary>
    public interface IHandCount
    {
        /// <summary>
        ///     Increment hand count
        /// </summary>
        void IncrementHandCount();

        /// <summary>
        ///     Send HandCountChangedEvent
        /// </summary>
        void SendHandCountChangedEvent();
    }
}
