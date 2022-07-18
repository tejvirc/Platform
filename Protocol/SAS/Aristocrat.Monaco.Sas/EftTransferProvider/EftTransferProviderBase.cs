namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    /// Definition of EftTransferProviderBase
    /// </summary>
    public abstract class EftTransferProviderBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The Sas client </summary>
        protected ITransactionCoordinator TransactionCoordinator { get; }

        /// <summary>Gets or sets the persisted transaction id.</summary>
        protected Guid TransactionId { get; set; } = Guid.Empty;

        /// <summary>
        /// PropertiesManager used to get the property value by the defined keys
        /// </summary>
        protected IPropertiesManager Properties { get; }

        /// <summary>
        /// Creates an instance of EftTransferProviderBase
        /// </summary>
        protected EftTransferProviderBase(
            ITransactionCoordinator transactionCoordinator,
            IPropertiesManager properties)
        {
            TransactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// Initializes the service
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>Releases the transaction guid, if any.</summary>
        protected void ReleaseTransactionId()
        {
            if (TransactionId != Guid.Empty)
            {
                Logger.Debug($"Releasing transaction ID: {TransactionId}.");
                TransactionCoordinator.ReleaseTransaction(TransactionId);
                TransactionId = Guid.Empty;
            }
            else
            {
                Logger.Warn("Trying to release an empty transaction!");
            }
        }

        /// <summary>
        /// Gets or sets whether or not partial transfers are allowed
        /// </summary>
        protected bool PartialTransferAllowed => (Properties.GetProperty(SasProperties.SasFeatureSettings, default(SasFeatures)) as SasFeatures)?.PartialTransferAllowed ?? false;

        /// <summary>
        /// Gets the name from the service.
        /// </summary>
        /// <returns>The name of the service.</returns>
        public string Name => ServiceTypes?.ElementAt(0).ToString();

        /// <summary>
        ///     Gets the type from a service.
        /// </summary>
        /// <returns>The type of the service.</returns>
        public abstract ICollection<Type> ServiceTypes { get; }

        /// <summary>
        /// Gets a value indicating whether or not the provider can accept On/Off transfers
        /// </summary>
        public virtual bool CanTransfer { get;  }
    }
}