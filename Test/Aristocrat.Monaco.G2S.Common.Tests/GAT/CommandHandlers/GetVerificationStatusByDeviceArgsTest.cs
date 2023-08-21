namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Common.GAT.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetVerificationStatusByDeviceArgsTest
    {
        private const long ValidVerificationId = 1;

        private const int ValidDeviceId = 2;

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenConstructWithInvalidDeviceIdExpectException()
        {
            var getVerificationStatusByDeviceArgs = new GetVerificationStatusByDeviceArgs(-1, ValidVerificationId);

            Assert.IsNull(getVerificationStatusByDeviceArgs);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenConstructWithInvalidVerificationIdExpectException()
        {
            var getVerificationStatusByDeviceArgs = new GetVerificationStatusByDeviceArgs(ValidDeviceId, -1);

            Assert.IsNull(getVerificationStatusByDeviceArgs);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectValidPropertiesSet()
        {
            var getVerificationStatusByDeviceArgs =
                new GetVerificationStatusByDeviceArgs(ValidDeviceId, ValidVerificationId);

            Assert.AreEqual(ValidVerificationId, getVerificationStatusByDeviceArgs.VerificationId);
            Assert.AreEqual(ValidDeviceId, getVerificationStatusByDeviceArgs.DeviceId);
        }
    }
}