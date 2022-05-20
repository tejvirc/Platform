namespace Aristocrat.Monaco.G2S.Services
{
    using System.Collections.Generic;
    using Accounting.Contracts.Transactions;
    using Aristocrat.G2S.Protocol.v21;

    public interface ITransactionReferenceProvider
    {
        IEnumerable<T> GetReferences<T>(ITransactionConnector connector)
            where T : c_transactionReference, new();
    }
}