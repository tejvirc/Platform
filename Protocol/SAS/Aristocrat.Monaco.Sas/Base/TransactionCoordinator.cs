namespace Aristocrat.Monaco.Sas.Base
{
    using System;
    using log4net;

    using Kernel;

    /// <summary>
    /// Definition of the TransactionCoordinator class.
    /// </summary>
    public class TransactionCoordinator : Contracts.Client.ISasTransactionCoordinator
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Guid _sasTransactionCoordinatorGuid = new Guid("{77BB97AD-4A86-40f4-87D7-8E59917697BA}");

        /// <inheritdoc />
        public Guid StartTransaction(int timeout)
        {
            Logger.Info("Starting a transaction.");

            Accounting.Contracts.ITransactionCoordinator transactionCoordinator;
            if (ServiceManager.GetInstance().IsServiceAvailable<Accounting.Contracts.ITransactionCoordinator>() &&
                (transactionCoordinator = ServiceManager.GetInstance().GetService<Accounting.Contracts.ITransactionCoordinator>()) != null)
            {
                return transactionCoordinator.RequestTransaction(_sasTransactionCoordinatorGuid, timeout, Accounting.Contracts.TransactionType.Read);
            }

            Logger.Warn("ITransactionCoordinator unavailable.");

            return Guid.Empty;
        }

        /// <inheritdoc />
        public void EndTransaction(Guid transactionId)
        {
            Logger.Info("Ending a transaction.");

            Accounting.Contracts.ITransactionCoordinator transactionCoordinator;
            if (ServiceManager.GetInstance().IsServiceAvailable<Accounting.Contracts.ITransactionCoordinator>() &&
                (transactionCoordinator = ServiceManager.GetInstance().GetService<Accounting.Contracts.ITransactionCoordinator>()) != null)
            {
                transactionCoordinator.ReleaseTransaction(transactionId);
            }
        }
    }
}
