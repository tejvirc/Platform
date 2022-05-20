namespace Aristocrat.Monaco.Application.Tests.Identification
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Identification;
    using Aristocrat.Monaco.Application.Identification;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class EmployeeLoginServiceTest
    {
        private const string _loginId = "Tester12345";
        private const string _loginIdDifferent = "Different";

        private Mock<IEventBus> _eventBus;
        private int _countEmployeeLoggedInEvents;
        private int _countEmployeeLoggedOutEvents;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _countEmployeeLoggedInEvents = 0;
            _countEmployeeLoggedOutEvents = 0;

            _eventBus = new Mock<IEventBus>();

            _eventBus.Setup(
                    b => b.Publish(It.IsAny<EmployeeLoggedInEvent>()))
                .Callback<object>(_ => _countEmployeeLoggedInEvents++);
            _eventBus.Setup(
                    b => b.Publish(It.IsAny<EmployeeLoggedOutEvent>()))
                .Callback<object>(_ => _countEmployeeLoggedOutEvents++);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var service = new EmployeeLoginService(null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenLoginExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 0);
        }

        [TestMethod]
        public void WhenLoginTwiceExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);
            service.Login(_loginId);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 0);
        }

        [TestMethod]
        public void WhenLoginDifferentExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);
            service.Login(_loginIdDifferent);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 0);
        }

        [TestMethod]
        public void WhenLogoutExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);

            service.Logout(_loginId);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 1);
        }

        [TestMethod]
        public void WhenLogoutTwiceExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);

            service.Logout(_loginId);
            service.Logout(_loginId);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 1);
        }

        [TestMethod]
        public void WhenLogoutDifferentExpectSuccess()
        {
            var service = new EmployeeLoginService(_eventBus.Object);

            service.Login(_loginId);

            service.Logout(_loginIdDifferent);

            Assert.AreEqual(_countEmployeeLoggedInEvents, 1);
            Assert.AreEqual(_countEmployeeLoggedOutEvents, 0);
        }
    }
}
