namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Definition of the ICurrency interface.
    /// </summary>
    public interface ICurrency
    {
        /// <summary>
        /// Gets the currency code string.
        /// </summary>
        /// <returns>The currency code the client is using.</returns>
        string CurrencyCode { get; }
    }
}
