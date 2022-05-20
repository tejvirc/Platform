using Aristocrat.Monaco.Application.Contracts;
using System.Collections.Generic;

namespace Aristocrat.Monaco.Accounting.Contracts.SelfAudit
{
    /// <summary>
    /// Debit meters are meters that are increased after calling Bank.Withdraw()
    /// Provides all debit side meters.
    /// </summary>
    public interface IDebitMetersProvider
    {
        /// <summary>
        /// Get all debit side meters for the layer.
        /// </summary>
        /// <returns>all debit side meters from implementing layer</returns>
        IEnumerable<IMeter> GetMeters();
    }
}
