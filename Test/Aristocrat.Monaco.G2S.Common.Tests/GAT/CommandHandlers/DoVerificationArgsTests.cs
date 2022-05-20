namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Authentication;
    using Common.GAT.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class DoVerificationArgsTests
    {
        private const long VerificationId = 1;

        private const int DeviceId = 2;

        private const string EmployeeId = "employee_id";

        private readonly Action<long> _queueVerifyCallBack = arg => { };

        private readonly Action<long> _startVerifyCallBack = arg => { };

        private readonly Action<DoVerificationArgs> _verificationCallback = arg => { };

        private readonly IEnumerable<VerifyComponent> _verifyComponents = new List<VerifyComponent>
        {
            new VerifyComponent("id", AlgorithmType.Crc16, string.Empty, string.Empty, 1, 1)
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfVerifyComponentsIsNull()
        {
            new DoVerificationArgs(VerificationId, DeviceId, EmployeeId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfVerificationIdLessThanOne()
        {
            new DoVerificationArgs(-1, DeviceId, EmployeeId, _verifyComponents);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowIfVerificationIdZero()
        {
            new DoVerificationArgs(0, DeviceId, EmployeeId, _verifyComponents);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowIfVerifyComponentsIsEmpty()
        {
            new DoVerificationArgs(VerificationId, DeviceId, EmployeeId, new List<VerifyComponent>());
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectValidPropertiesSet()
        {
            var doVerificationArgs = new DoVerificationArgs(
                VerificationId,
                DeviceId,
                EmployeeId,
                _verifyComponents,
                _verificationCallback,
                _queueVerifyCallBack,
                _startVerifyCallBack);

            Assert.AreEqual(VerificationId, doVerificationArgs.VerificationId);
            Assert.AreEqual(DeviceId, doVerificationArgs.DeviceId);
            Assert.AreEqual(EmployeeId, doVerificationArgs.EmployeeId);
            Assert.AreEqual(_verifyComponents, doVerificationArgs.VerifyComponents);
            Assert.AreEqual(_verificationCallback, doVerificationArgs.VerificationCallback);
            Assert.AreEqual(_startVerifyCallBack, doVerificationArgs.StartVerifyCallback);
        }
    }
}
