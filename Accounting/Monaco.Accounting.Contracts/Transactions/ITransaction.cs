namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Application.Contracts;

    /// <summary>
    ///     Defines the fundamental properties that encompass all types of transactions.
    /// </summary>
    /// <remarks>
    ///     Transactions of all types are saved and retrieved from the Transaction History service. This interface makes a
    ///     common set of properties accessible. It contains properties for recurring parameters in G2S transactions.
    /// </remarks>
    public interface ITransaction : ILogSequence, ICloneable
    {
        /// <summary>
        ///     Gets the device identifier of the device that generated the transaction. Wildcards not permitted.
        /// </summary>
        int DeviceId { get; }

        /// <summary>
        ///     Gets or sets the unique transaction identifier assigned by the EGM.
        /// </summary>
        long TransactionId { get; set; }

        /// <summary>
        ///     Gets the Date/time that the transaction took place.
        /// </summary>
        DateTime TransactionDateTime { get; }

        /// <summary>
        ///     Gets the human readable name of the transaction type
        /// </summary>
        string Name { get; }
    }
}