namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Tests for the LP54SendSasVersionHandler class
    /// </summary>
    [TestClass]
    public class LP54SendSasVersionHandlerTest
    {
        private LP54SendSasVersionHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _target = new LP54SendSasVersionHandler(_propertiesManager.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendSasVersionAndGameSerial));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPropertiesProviderTest()
        {
            _target = new LP54SendSasVersionHandler(null);
        }

        [DataRow(
            "603",
            "1234",
            "1234",
            DisplayName = "Using the string serial number when it is set to valid value")]
        [DataRow(
            "603",
            "",
            "",
            DisplayName = "Using the int serial number when the string serial number is empty")]
        [DataRow(
            "603",
            (string)null,
            (string)null,
            DisplayName = "Using the int serial number when the string serial number is null")]
        [DataTestMethod]
        public void HandleTest(
            string sasVersion,
            string serialNumber,
            string expectedSerialNumber)
        {
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasVersion, It.IsAny<object>()))
                .Returns(sasVersion);

            var expected = _target.Handle(new LongPollData());
            Assert.AreEqual(expected.SasVersion, sasVersion);
            Assert.AreEqual(expected.SerialNumber, expectedSerialNumber);
        }
    }
}
