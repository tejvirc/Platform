namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Sas.Contracts.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Moq;

    public static class AftTransferProviderExtensions
    {
        public static void SetupErrorHandler(this Mock<IAftTransferProvider> aftProvider, AftResponseData reponseData)
        {
            aftProvider.Setup(x => x.CheckForErrorConditions(It.IsAny<Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>>()))
                .Callback((Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> e) => CheckErrors(e, aftProvider, reponseData));
        }

        private static void CheckErrors(
            Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> errorConditions,
            Mock<IAftTransferProvider> aftProvider,
            AftResponseData data)
        {
            foreach (var condition in errorConditions)
            {
                if (condition.Key.Invoke())
                {
                    data.TransferStatus = condition.Value.code;
                    data.ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested;
                    data.CashableAmount = 0;
                    data.NonRestrictedAmount = 0;
                    data.RestrictedAmount = 0;
                    data.TransactionDateTime = DateTime.MaxValue;
                    aftProvider.Setup(x => x.TransferFailure).Returns(true);
                    return;
                }
            }

            aftProvider.Setup(x => x.TransferFailure).Returns(false);
        }
    }
}
