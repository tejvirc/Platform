namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Common.GAT.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetVerificationStatusArgsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfVerificationIdLessThanOne()
        {
            this.CreateVerificationStatus(1, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfVerificationIdZero()
        {
            this.CreateVerificationStatus(1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfTransactionIdLessThanOne()
        {
            this.CreateVerificationStatus(-1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfTransactionIdZero()
        {
            this.CreateVerificationStatus(0, 1);
        }

        private void CreateVerificationStatus(Int64 transactionId, Int64 verificationId)
        {
            new GetVerificationStatusByTransactionArgs(transactionId, verificationId);
        }
    }
}