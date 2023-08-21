namespace Aristocrat.Monaco.Sas.Base
{
    using Kernel;

    /// <inheritdoc />
    public class Currency : Contracts.Client.ICurrency
    {
        /// <summary>
        /// Initializes a new instance of the Currency class.
        /// </summary>
        public Currency()
        {
            var serviceManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            CurrencyCode = serviceManager?.GetProperty("Currency", string.Empty).ToString();
        }

        /// <inheritdoc />
        public string CurrencyCode { get; }
    }
}
