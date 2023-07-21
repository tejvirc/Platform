namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Mgam.Common.Events;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Commands;
    using Mgam.Services.Lockup;
    using Mgam.Services.Security;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ChecksumCalculatorTest
    {
        private const int waitTimeout = 5000;
        private Mock<ILogger<ChecksumCalculator>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<ILockup> _lockup;
        private Mock<INotificationLift> _notificationLift;
        private Mock<IEventBus> _bus;
        private Mock<IGameProvider> _gameProvider;
        private Func<AttributesUpdatedEvent, CancellationToken, Task> _handler;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _logger = new Mock<ILogger<ChecksumCalculator>>();
            _commandFactory = new Mock<ICommandHandlerFactory>();
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            _authenticationService = new Mock<IAuthenticationService>();
            _lockup = new Mock<ILockup>();
            _notificationLift = new Mock<INotificationLift>();
            _bus = new Mock<IEventBus>(MockBehavior.Strict);

            _bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<HostOfflineEvent>>()));
            _bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Func<AttributesUpdatedEvent, CancellationToken, Task>>()))
                    .Callback<object, Func<AttributesUpdatedEvent, CancellationToken, Task>>((_, handler) =>
                    {
                        _handler = handler;
                    });

            _gameProvider = new Mock<IGameProvider>();
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(new List<IGameDetail>());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLoggerExpectException()
        {
            var service = new ChecksumCalculator(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandFactoryExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullUnitOfWorkFactoryExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullAuthenticationExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLockupExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullNotifierExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullBusExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                _notificationLift.Object,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                _notificationLift.Object,
                _bus.Object,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                _notificationLift.Object,
                _bus.Object,
                _gameProvider.Object);

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(IChecksumCalculator));
        }

        [TestMethod]
        public void CalculateTestSuccess()
        {
            var done = new ManualResetEventSlim(false);

            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                _notificationLift.Object,
                _bus.Object,
                _gameProvider.Object);

            _commandFactory.Setup(c => c.Execute(It.IsAny<Commands.Checksum>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _notificationLift.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            var test = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Common.Data.Models.Checksum { Seed = 0 });

            test.repo.Setup(r => r.Delete(It.IsAny<long>()))
                .Callback<long>(_ =>
                {
                    done.Set();
                })
                .Verifiable();

            Assert.IsNotNull(_handler);

            _handler.Invoke(new AttributesUpdatedEvent(), CancellationToken.None).Wait();

            if (!done.Wait(waitTimeout))
            {
                Assert.Fail("Timed out waiting for test to complete");
            }

            test.repo.Verify();
            _commandFactory.Verify();
        }

        [TestMethod]
        public void CalculateTestFailure()
        {
            var done = new ManualResetEventSlim(false);

            var service = new ChecksumCalculator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _authenticationService.Object,
                _lockup.Object,
                _notificationLift.Object,
                _bus.Object,
                _gameProvider.Object);

            _commandFactory.Setup(c => c.Execute(It.IsAny<Commands.Checksum>()))
                .Throws(new ServerResponseException(ServerResponseCode.ChecksumFailure)).Verifiable();

            _notificationLift.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            var test = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Common.Data.Models.Checksum { Seed = 0 });

            test.repo.Setup(r => r.Delete(It.IsAny<long>()))
                .Callback<long>(_ =>
                {
                    done.Set();
                })
                .Verifiable();

            Assert.IsNotNull(_handler);

            _handler.Invoke(new AttributesUpdatedEvent(), CancellationToken.None).Wait();

            if (!done.Wait(waitTimeout))
            {
                Assert.Fail("Timed out waiting for test to complete");
            }

            test.repo.Verify();
            _commandFactory.Verify();
        }
    }
}