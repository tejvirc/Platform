namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts.Wat;
    using Kernel;

    /// <summary>
    ///     Definition of the TestWatOffProvider class.
    /// </summary>
    public class TestWatOffProvider : IWatTransferOffProvider, IService
    {
        public string Name => typeof(TestWatOffProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IWatTransferOffProvider) };

        public void Initialize()
        {
        }

        public bool CanTransfer { get; } = false;

        public Task<bool> InitiateTransfer(WatTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!CanTransfer)
            {
                return Task.FromResult(false);
            }

            // Just to show we can run this in a separate thread
            return Task.Run(
                () =>
                {
                    // Normally this would send something to the WAT host, but we're just going to ack it here

                    transaction.PayMethod = WatPayMethod.Credit;
                    transaction.AuthorizedCashableAmount = transaction.CashableAmount;
                    transaction.AuthorizedPromoAmount = transaction.PromoAmount;
                    transaction.AuthorizedNonCashAmount = transaction.AuthorizedNonCashAmount;

                    return true;
                });
        }

        public Task CommitTransfer(WatTransaction transaction)
        {
            return Task.Run(
                () => { ServiceManager.GetInstance().GetService<IWatOffProvider>().AcknowledgeTransfer(transaction); });
        }
    }
}