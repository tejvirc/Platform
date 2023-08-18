namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class SetTicketDataHandlerTests
    {
        private const int Timeout = 5000; // five seconds
        private ManualResetEvent _waiter;
        private SetTicketDataHandler _target;
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<ITicketDataProvider> _ticketDataProvider = new Mock<ITicketDataProvider>(MockBehavior.Default);
        private readonly Mock<ITicketingCoordinator> _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
        private readonly Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation());
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());

            _waiter = new ManualResetEvent(false);
            _target = CreateTarget();
        }

        private SetTicketDataHandler CreateTarget(
            bool nullProperties = false,
            bool nullValidationDataProvider = false,
            bool nullTicketDataProvider = false,
            bool nullTicketingCoordinator = false,
            bool nullUnitOfWOrkFactory = false)
        {
            return new SetTicketDataHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullValidationDataProvider ? null : _validationDataProvider.Object,
                nullTicketDataProvider ? null : _ticketDataProvider.Object,
                nullTicketingCoordinator ? null : _ticketingCoordinator.Object,
                nullUnitOfWOrkFactory ? null : _unitOfWorkFactory.Object);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(
            bool nullProperties,
            bool nullValidationDataProvider,
            bool nullTicketDataProvider,
            bool nullTicketingCoordinator,
            bool nullUnitOfWOrkFactory)
        {
            _target = CreateTarget(
                nullProperties,
                nullValidationDataProvider,
                nullTicketDataProvider,
                nullTicketingCoordinator,
                nullUnitOfWOrkFactory);
        }

        [DataRow(LongPoll.SetTicketData)]
        [DataRow(LongPoll.SetExtendedTicketData)]
        [DataTestMethod]
        public void CommandsTest(LongPoll command)
        {
            Assert.AreEqual(2, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(command));
        }

        [DataRow(
            true,
            true,
            true,
            true,
            true,
            DisplayName = "All values can be cleared")]
        [DataRow(
            true,
            false,
            false,
            false,
            false,
            DisplayName = "Only Location is valid")]
        [DataRow(
            false,
            true,
            false,
            false,
            false,
            DisplayName = "Only Address1 is valid")]
        [DataRow(
            false,
            false,
            true,
            false,
            false,
            DisplayName = "Only Address2 is valid")]
        [DataRow(
            false,
            false,
            false,
            true,
            false,
            DisplayName = "Only restricted ticket title is valid")]
        [DataRow(
            false,
            false,
            false,
            false,
            true,
            DisplayName = "Only debit ticket title is valid")]
        [DataTestMethod]
        public void ClearValidationData(
            bool clearLocation,
            bool clearAddress1,
            bool clearAddress2,
            bool clearRestrictedTicketTitle,
            bool clearDebitTicketTitle)
        {
            const string location = "location";
            const string address1 = "address1";
            const string address2 = "address2";
            const string restrictedTicketTitle = " restricted";
            const string debitTicketTitle = "debit";

            var ticketData = new TicketData();
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ExtendedTicketDataSet = false,
                ExtendedTicketDataStatus = TicketDataStatus.ValidData
            });

            _validationDataProvider.Setup(x =>
                x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.ValidData
                                                         && v.ExtendedTicketDataSet), _unitOfWork.Object))
                .Verifiable();
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultLocationKey, It.IsAny<string>()))
                .Returns(location);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultAddressLine1Key, It.IsAny<string>()))
                .Returns(address1);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultAddressLine2Key, It.IsAny<string>()))
                .Returns(address2);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultRestrictedTitleKey, It.IsAny<string>()))
                .Returns(restrictedTicketTitle);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultDebitTitleKey, It.IsAny<string>()))
                .Returns(debitTicketTitle);

            _unitOfWork.Setup(x => x.Commit()).Callback(() => _waiter.Set());
            _ticketDataProvider.Setup(x => x.TicketData).Returns(ticketData).Verifiable();
            _ticketDataProvider.Setup(x => x.SetTicketData(It.IsAny<TicketData>()))
                .Callback((TicketData data) => ticketData = data)
                .Verifiable();

            var message = new SetTicketData
            {
                ValidTicketData = true,
                Location = location,
                Address1 = address1,
                Address2 = address2,
                RestrictedTicketTitle = restrictedTicketTitle,
                DebitTicketTitle = debitTicketTitle,
                IsExtendTicketData = true
            };

            var response = _target.Handle(message);

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            Assert.AreEqual(TicketDataStatus.ValidData, response.Data);
            if (clearLocation)
            {
                Assert.AreEqual(location, ticketData.Location);
            }

            if (clearAddress1)
            {
                Assert.AreEqual(address1, ticketData.Address1);
            }

            if (clearAddress2)
            {
                Assert.AreEqual(address2, ticketData.Address2);
            }

            if (clearRestrictedTicketTitle)
            {
                Assert.AreEqual(restrictedTicketTitle, ticketData.RestrictedTicketTitle);
            }

            if (clearDebitTicketTitle)
            {
                Assert.AreEqual(debitTicketTitle, ticketData.DebitTicketTitle);
            }

            _ticketDataProvider.Verify();
            _propertiesManager.Verify();
        }

        [DataRow(
            "location",
            "address1",
            "address2",
            "restricted",
            "debit",
            0,
            false,
            true,
            DisplayName = "All values can be sent")]
        [DataRow(
            "location",
            null,
            null,
            null,
            null,
            0,
            false,
            true,
            DisplayName = "Only Location is valid")]
        [DataRow(
            null,
            "address1",
            null,
            null,
            null,
            0,
            false,
            true,
            DisplayName = "Only Address1 is valid")]
        [DataRow(
            null,
            null,
            "address2",
            null,
            null,
            0,
            false,
            true,
            DisplayName = "Only Address2 is valid")]
        [DataRow(
            null,
            null,
            null,
            "restricted",
            null,
            0,
            false,
            true,
            DisplayName = "Only restricted ticket title is valid")]
        [DataRow(
            null,
            null,
            null,
            null,
            "debit",
            0,
            false,
            true,
            DisplayName = "Only debit ticket title is valid")]
        [DataRow(
            null,
            null,
            null,
            null,
            null,
            50,
            true,
            false,
            DisplayName = "Only expiration date is valid")]
        [DataRow(
            null,
            null,
            null,
            null,
            null,
            0,
            true,
            false,
            DisplayName = "Zero expiration returns 9999 for the expiration")]
        [DataTestMethod]
        public void ValidDataHandler(
            string location,
            string address1,
            string address2,
            string restrictedTicketTitle,
            string debitTicketTitle,
            int expirationDate,
            bool setExpirationDate,
            bool isExtendTicketData)
        {
            var expectedExpirationDate = expirationDate > 0 ? expirationDate : SasConstants.MaxTicketExpirationDays;
            var ticketData = new TicketData();

            if (isExtendTicketData)
            {
                _validationDataProvider.Setup(x =>
                    x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataSet), _unitOfWork.Object))
                    .Verifiable();
            }
            else
            {
                _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation { ExtendedTicketDataSet = false });
            }

            if (setExpirationDate)
            {
                _ticketingCoordinator.Setup(x => x.Save(
                    It.Is<TicketStorageData>(t => t.RestrictedTicketCombinedExpiration == expectedExpirationDate && t.TicketExpiration == expectedExpirationDate),
                    It.IsAny<IUnitOfWork>())).Verifiable();
            }

            _unitOfWork.Setup(x => x.Commit()).Callback(() => _waiter.Set());
            _ticketDataProvider.Setup(x => x.TicketData).Returns(ticketData).Verifiable();
            _ticketDataProvider.Setup(x => x.SetTicketData(It.IsAny<TicketData>()))
                .Callback((TicketData data) => ticketData = data)
                .Verifiable();

            var message = new SetTicketData
            {
                ValidTicketData = true,
                Location = location,
                Address1 = address1,
                Address2 = address2,
                RestrictedTicketTitle = restrictedTicketTitle,
                DebitTicketTitle = debitTicketTitle,
                ExpirationDate = expirationDate,
                SetExpirationDate = setExpirationDate,
                IsExtendTicketData = isExtendTicketData
            };

            var response = _target.Handle(message);

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            Assert.AreEqual(TicketDataStatus.ValidData, response.Data);
            if (location != null)
            {
                Assert.AreEqual(location, ticketData.Location);
            }

            if (address1 != null)
            {
                Assert.AreEqual(address1, ticketData.Address1);
            }

            if (address2 != null)
            {
                Assert.AreEqual(address2, ticketData.Address2);
            }

            if (restrictedTicketTitle != null)
            {
                Assert.AreEqual(restrictedTicketTitle, ticketData.RestrictedTicketTitle);
            }

            if (debitTicketTitle != null)
            {
                Assert.AreEqual(debitTicketTitle, ticketData.DebitTicketTitle);
            }

            _ticketDataProvider.Verify();
            _propertiesManager.Verify();
            _ticketingCoordinator.Verify();
            _unitOfWorkFactory.Verify();
        }

        [TestMethod]
        public void SettingLegacyTicketDataWhenExtendedDataIsSet()
        {
            var message = new SetTicketData
            {
                ValidTicketData = true,
                ExpirationDate = 100,
                SetExpirationDate = true,
                IsExtendTicketData = false
            };

            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.InvalidData)))
                .Returns(Task.CompletedTask)
                .Callback(() => _waiter.Set());
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation { ExtendedTicketDataSet = true });
            var response = _target.Handle(message);
            Assert.AreEqual(TicketDataStatus.InvalidData, response.Data);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
        }

        [TestMethod]
        public void InvalidDataHandler()
        {
            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.InvalidData)))
                .Returns(Task.CompletedTask)
                .Callback(() => _waiter.Set());
            var response = _target.Handle(new SetTicketData { ValidTicketData = false });

            Assert.AreEqual(response.Data, TicketDataStatus.InvalidData);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
        }

        [TestMethod]
        public void AckNackHandlerTest()
        {
            var data = new SetTicketData
            {
                ValidTicketData = true, BroadcastPoll = false, SetExpirationDate = false, IsExtendTicketData = false
            };

            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.InvalidData)))
                .Returns(Task.CompletedTask)
                .Callback(() => _waiter.Set());
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation() { ExtendedTicketDataSet = false });
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, true)).Verifiable();
            _ticketDataProvider.Setup(m => m.TicketData).Returns(new TicketData());

            var result = _target.Handle(data);

            // mocks for ack handler
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.ExtendedTicketDataStatusClearPending, false)).Returns(true);
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, false)).Verifiable();

            Assert.IsNotNull(result.Handlers.ImpliedAckHandler);
            result.Handlers.ImpliedAckHandler.Invoke();

            // mocks for nack handler
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, false)).Verifiable();

            Assert.IsNotNull(result.Handlers.ImpliedNackHandler);
            result.Handlers.ImpliedNackHandler.Invoke();
            Assert.IsTrue(_waiter.WaitOne(Timeout));

            _propertiesManager.Verify();
            _ticketDataProvider.Verify();
        }

        [TestMethod]
        public void NullAckNackHandlerTest()
        {
            var data = new SetTicketData { ValidTicketData = false };

            var result = _target.Handle(data);

            Assert.IsNull(result.Handlers.ImpliedAckHandler);
            Assert.IsNull(result.Handlers.ImpliedNackHandler);
        }
    }
}