namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aft;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    [TestClass]
    public class LP75SetAftReceiptDataHandlerTest
    {
        private LP75SetAftReceiptDataHandler _target;

        private readonly Mock<IPropertiesManager>
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private SetAftReceiptData _data;
        private const string testValue = "Test Value";

        // Not the same as SasConstants.UseDefault
        private const string defaultValue = "Default Value";

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(It.IsAny<string>(), It.IsAny<string>())).Returns(defaultValue);
            _propertiesManager.Setup(x => x.SetProperty(It.IsAny<string>(), It.IsAny<string>()));

            _data = new SetAftReceiptData();

            _target = new LP75SetAftReceiptDataHandler(_propertiesManager.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SetAftReceiptData));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP75SetAftReceiptDataHandler(null);
        }

        [TestMethod]
        public void HandleNoSettingsTest()
        {
            // All data strings are null, so SetProperty should never be called.
            _target.Handle(_data);

            _propertiesManager.Verify(x => x.SetProperty(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void HandleUseDefaultsTest()
        {
            int totalPropertiesToSet = 11;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Location] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Address1] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Address2] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse1] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse2] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse3] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse4] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit1] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit2] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit3] = SasConstants.UseDefault;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit4] = SasConstants.UseDefault;

            _target.Handle(_data);

            _propertiesManager.Verify(
                x => x.SetProperty(It.IsAny<string>(), defaultValue),
                Times.Exactly(totalPropertiesToSet));
        }

        [TestMethod]
        public void HandleUseValuesTest()
        {
            int totalPropertiesToSet = 11;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Location] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Address1] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Address2] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse1] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse2] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse3] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.InHouse4] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit1] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit2] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit3] = testValue;
            _data.TransactionReceiptValues[TransactionReceiptDataField.Debit4] = testValue;

            _target.Handle(_data);

            _propertiesManager.Verify(
                x => x.SetProperty(It.IsAny<string>(), testValue),
                Times.Exactly(totalPropertiesToSet));
        }
    }
}