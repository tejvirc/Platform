namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Globalization;
    using Hardware.Contracts.Persistence;
    using Mono.Addins;

#pragma warning disable 0649

    /// <summary>
    ///     Definition of the TransactionHistoryProviderExtensionNode class.
    /// </summary>
    [CLSCompliant(false)]
    public class TransactionHistoryProviderExtensionNode : TypeExtensionNode
    {
        /// <summary>
        ///     Indicates whether or not this transaction provider is for a printable voucher.
        /// </summary>
        [NodeAttribute("isPrintable", false)] private bool _isPrintable;

        /// <summary>
        ///     Specifies the maximum number of transactions the provider holds.
        /// </summary>
        [NodeAttribute("maxTransactions", true)] private string _maxTransactions;

        /// <summary>
        ///     Specifies the persistence level to be used for PersistenStorage.
        /// </summary>
        [NodeAttribute("persistenceLevel", true)] private string _persistenceLevel;

        /// <summary>
        ///     Gets the maximum number of transactions the provider holds.
        /// </summary>
        public int MaxTransactions => Convert.ToInt32(_maxTransactions, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Gets the storage media type used for PersistenStorage.
        /// </summary>
        public PersistenceLevel Level => (PersistenceLevel)Enum.Parse(typeof(PersistenceLevel), _persistenceLevel);

        /// <summary>
        ///     Gets a value indicating whether this transaction provider is for a printable voucher.
        /// </summary>
        public bool IsPrintable => Convert.ToBoolean(_isPrintable, CultureInfo.InvariantCulture);
    }

#pragma warning restore 0649
}