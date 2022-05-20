namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.Aft;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains tests for the LP73AftRegisterGamingMachineParser class
    /// </summary>
    [TestClass]
    public class LP73AftRegisterGamingMachineParserTest
    {
        private readonly Mock<ISasLongPollHandler<AftRegisterGamingMachineResponseData, AftRegisterGamingMachineData>> _handler = new Mock<ISasLongPollHandler<AftRegisterGamingMachineResponseData, AftRegisterGamingMachineData>>();
        private LP73AftRegisterGamingMachineParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP73AftRegisterGamingMachineParser();
            _target.InjectHandler(_handler.Object);

            var response = new AftRegisterGamingMachineResponseData();
            _handler.Setup(x => x.Handle(It.IsAny<AftRegisterGamingMachineData>())).Returns(response);
        }

        [TestMethod]
        public void ParseWithTooFewBytesTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x01,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>();

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithMismatchedLengthTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x1D, (byte)AftRegistrationCode.ReadCurrentRegistration,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>();

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithInvalidLengthTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x02, (byte)AftRegistrationCode.ReadCurrentRegistration, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>();

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithInterrogateLengthButNotInterrogateTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x01, (byte)AftRegistrationCode.RegisterGamingMachine,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>();

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [DataRow(AftRegistrationCode.ReadCurrentRegistration)]
        [DataRow(AftRegistrationCode.UnregisterGamingMachine)]
        [DataTestMethod]
        public void ParseShortCommandsTest(AftRegistrationCode registrationCode)
        {
            var response = new AftRegisterGamingMachineResponseData();
            _handler.Setup(x => x.Handle(It.IsAny<AftRegisterGamingMachineData>())).Returns(response);

            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x01, (byte)registrationCode,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x73, 0x1D,
                0,0,0,0,0,0,0,0,0,0, // 0x1D more bytes
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [DataRow(AftRegistrationCode.InitializeRegistration)]
        [DataRow(AftRegistrationCode.RegisterGamingMachine)]
        [DataRow(AftRegistrationCode.RequestOperatorAcknowledgement)]
        [DataTestMethod]
        public void ParseLongCommandsTest(AftRegistrationCode registrationCode)
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x73,
                0x1D, (byte)registrationCode,
                0x01, 0x02, 0x03, 0x04,
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20,
                0xB3, 0x74, 0xA4, 0x02,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x73, 0x1D,
                0,0,0,0,0,0,0,0,0,0, // 0x1D more bytes
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
