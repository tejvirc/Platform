namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
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
    public class LP4CSetValidationIdNumberHandlerTest
    {
        private const int WaitTime = 1000;

        private LP4CSetValidationIdNumberHandler _target;
        private Mock<ISasDisableProvider> _disableProvider;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider;
        private AutoResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _disableProvider = new Mock<ISasDisableProvider>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);
            _waiter = new AutoResetEvent(false);

            _target = new LP4CSetValidationIdNumberHandler(_propertiesManager.Object, _disableProvider.Object, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new LP4CSetValidationIdNumberHandler(null, _disableProvider.Object, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullDisableManagerTest()
        {
            _target = new LP4CSetValidationIdNumberHandler(_propertiesManager.Object, null, _validationDataProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullValidationDataProviderTest()
        {
            _target = new LP4CSetValidationIdNumberHandler(_propertiesManager.Object, _disableProvider.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SetSecureEnhancedValidationId));
        }

        [DataRow(true, (uint)0, (uint)0, (uint)0, (uint)123, (uint)456, DisplayName = "First time setting")]
        [DataRow(true, (uint)187, (uint)456, (uint)4, (uint)123, (uint)4, DisplayName = "Different Machine sets the sequence ID")]
        [DataRow(true, (uint)123, (uint)456, (uint)4, (uint)123, (uint)5, DisplayName = "Different Sequence number than previous message sets the sequence ID")]
        [DataRow(false, (uint)123, (uint)456, (uint)465, (uint)123, (uint)465, DisplayName = "Un-configured validation id is set")]
        [DataTestMethod]
        public void HandleSettingValuesTest(
            bool validationConfigured,
            uint currentMachineId,
            uint currentSequenceNumber,
            uint lastSetSequenceNumber,
            uint settingMachineNumber,
            uint settingSequenceNumber)
        {
            var data = new LongPoll4CData
            {
                MachineValidationId = settingMachineNumber,
                SequenceNumber = settingSequenceNumber
            };

            var expected = new LongPoll4CResponse
            {
                UsingSecureEnhancedValidation = true,
                SequenceNumber = settingSequenceNumber,
                MachineValidationId = settingMachineNumber
            };

            _validationDataProvider.Setup(x =>
                x.Save(It.Is<ValidationInformation>(v => v.SequenceNumber == data.SequenceNumber &&
                                                         v.MachineValidationId == data.MachineValidationId &&
                                                         v.LastReceivedSequenceNumber == data.SequenceNumber &&
                                                         v.ValidationConfigured)))
                .Returns(Task.CompletedTask)
                .Callback(() => _waiter.Set());
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                LastReceivedSequenceNumber = lastSetSequenceNumber,
                MachineValidationId = currentMachineId,
                SequenceNumber = currentSequenceNumber,
                ValidationConfigured = validationConfigured
            });

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });

            var actual = _target.Handle(data);

            Assert.IsTrue(_waiter.WaitOne(WaitTime));

            Assert.AreEqual(expected.UsingSecureEnhancedValidation, actual.UsingSecureEnhancedValidation);
            Assert.AreEqual(expected.MachineValidationId, actual.MachineValidationId);
            Assert.AreEqual(expected.SequenceNumber, actual.SequenceNumber);

            _propertiesManager.Verify();
        }

        [TestMethod]
        public void HandleNotUsingSecureEnhancedTest()
        {
            var data = new LongPoll4CData { MachineValidationId = 123, SequenceNumber = 456 };
            var expected = new LongPoll4CResponse
                { UsingSecureEnhancedValidation = false, SequenceNumber = 0, MachineValidationId = 0 };
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.None
            });

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.UsingSecureEnhancedValidation, actual.UsingSecureEnhancedValidation);
            Assert.AreEqual(expected.MachineValidationId, actual.MachineValidationId);
            Assert.AreEqual(expected.SequenceNumber, actual.SequenceNumber);
        }

        [DataRow((uint)123, (uint)456, (uint)4, (uint)123, (uint)4, DisplayName = "Sending the same information that was sent previously does not update")]
        [DataRow((uint)187, (uint)456, (uint)4, (uint)0, (uint)901, DisplayName = "Machine ID zero never sets data")]
        [DataTestMethod]
        public void HandleFailingSettingTest(
            uint currentMachineId,
            uint currentSequenceNumber,
            uint lastSetSequenceNumber,
            uint settingMachineNumber,
            uint settingSequenceNumber)
        {
            var data = new LongPoll4CData
            {
                MachineValidationId = settingMachineNumber,
                SequenceNumber = settingSequenceNumber
            };

            var expected = new LongPoll4CResponse
            {
                UsingSecureEnhancedValidation = true,
                SequenceNumber = currentSequenceNumber,
                MachineValidationId = currentMachineId
            };

            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                LastReceivedSequenceNumber = lastSetSequenceNumber,
                MachineValidationId = currentMachineId,
                SequenceNumber = currentSequenceNumber,
                ValidationConfigured = true
            });

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.UsingSecureEnhancedValidation, actual.UsingSecureEnhancedValidation);
            Assert.AreEqual(expected.MachineValidationId, actual.MachineValidationId);
            Assert.AreEqual(expected.SequenceNumber, actual.SequenceNumber);

            _propertiesManager.Verify();
        }

        [TestMethod]
        public void RollOverTest()
        {
            const uint machineId = 1;
            var data = new LongPoll4CData
            {
                MachineValidationId = 0,
                SequenceNumber = SasConstants.MaxValidationSequenceNumber
            };

            var expected = new LongPoll4CResponse
            {
                UsingSecureEnhancedValidation = true,
                SequenceNumber = 0,
                MachineValidationId = machineId
            };

            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                MachineValidationId = machineId,
                SequenceNumber = SasConstants.MaxValidationSequenceNumber + 1,
                ValidationConfigured = true
            });

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.UsingSecureEnhancedValidation, actual.UsingSecureEnhancedValidation);
            Assert.AreEqual(expected.MachineValidationId, actual.MachineValidationId);
            Assert.AreEqual(expected.SequenceNumber, actual.SequenceNumber);

            _propertiesManager.Verify();
        }
    }
}
