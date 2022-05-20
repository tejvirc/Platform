namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     This interface defines methods that allows to process parameters fields in one transaction.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        ///     Called at beginning of transaction. Use this to backup any state required for Rollback.
        /// </summary>
        /// <param name="dataMemberNames">
        ///     The specific data members that will be updated in this transaction. This is useful for datasources that are used across multiple device types and parameters.
        /// </param>
        void Begin(IReadOnlyList<string> dataMemberNames);

        /// <summary>
        ///     Commit all field updates since transaction creation.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Roll back all field updates since transaction creation.
        /// </summary>
        void RollBack();
    }
}