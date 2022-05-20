using Aristocrat.Monaco.Application.Contracts;
using System.Collections.Generic;

namespace Aristocrat.Monaco.Accounting.Contracts.SelfAudit
{
    /// <summary>
    /// Credit meters are those meters that are incremented after calling Bank.Deposit()
    /// Provides list of all credit side meters.
    /// </summary>
    public interface ICreditMetersProvider
    {
        /// <summary>
        /// Get all credit side meters for the layer.
        /// </summary>
        /// <returns>all credit side meters from implementing layer</returns>
        IEnumerable<IMeter> GetMeters();
    }
}
