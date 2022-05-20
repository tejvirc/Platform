namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Aft;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    [TestClass]
    public class LP73AftRegisterGamingMachineHandlerTest
    {
        private const uint AssetNumber = 6;
        private const uint PosId = 8;
        private const byte ExpectedResponseLength = 0x1D;
        private byte[] zeroRegistrationKey;
        private byte[] validRegistrationKey;
        private LP73AftRegisterGamingMachineHandler _target;
        private Mock<IAftRegistrationProvider> _aftRegistrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Strict);
        private Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private AftRegisterGamingMachineData data;
        private AftRegisterGamingMachineResponseData response;

        [TestInitialize]
        public void MyTestInitialize()
        {
            zeroRegistrationKey = new byte[20];
            validRegistrationKey = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            _aftRegistrationProvider.Setup(x => x.ZeroRegistrationKey).Returns(zeroRegistrationKey);
            _aftRegistrationProvider.Setup(x => x.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(true);
            _aftRegistrationProvider.Setup(x => x.RegistrationKeyMatches(zeroRegistrationKey, zeroRegistrationKey)).Returns(true);
            _aftRegistrationProvider.Setup(x => x.RegistrationKeyMatches(validRegistrationKey, validRegistrationKey)).Returns(true);
            _aftRegistrationProvider.Setup(x => x.RegistrationKeyMatches(validRegistrationKey, zeroRegistrationKey)).Returns(false);
            _aftRegistrationProvider.Setup(x => x.ProcessAftRegistration(It.IsAny<AftRegistrationCode>(), It.IsAny<uint>(), It.IsAny<byte[]>(), It.IsAny<uint>()));
            _aftRegistrationProvider.Setup(x => x.ForceAftNotRegistered());

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsCashOutDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsHandPayDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.AftCustomTicketsSupportedKey, It.IsAny<object>())).Returns(true);

            data = new AftRegisterGamingMachineData();

            _target = new LP73AftRegisterGamingMachineHandler(
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.AftRegisterGamingMachine));
        }

        [TestMethod]
        public void HandleReadCurrentRegistrationNotRegisteredTest()
        {
            DoSetups(false);

            data.RegistrationCode = AftRegistrationCode.ReadCurrentRegistration;
            response = _target.Handle(data);
            Assert.AreEqual(ExpectedResponseLength, response.Length);
            Assert.AreEqual(AftRegistrationStatus.NotRegistered, response.RegistrationStatus);
            Assert.AreEqual((uint)0, response.AssetNumber);
            Assert.AreEqual(zeroRegistrationKey, response.RegistrationKey);
            Assert.AreEqual((uint)0, response.PosId);
        }

        [TestMethod]
        public void HandleReadCurrentRegistrationRegisteredTest()
        {
            DoSetups(true);

            data.RegistrationCode = AftRegistrationCode.ReadCurrentRegistration;
            response = _target.Handle(data);
            Assert.AreEqual(ExpectedResponseLength, response.Length);
            Assert.AreEqual(AftRegistrationStatus.Registered, response.RegistrationStatus);
            Assert.AreEqual(AssetNumber, response.AssetNumber);
            Assert.AreEqual(validRegistrationKey, response.RegistrationKey);
            Assert.AreEqual(PosId, response.PosId);
        }


        [TestMethod]
        public void HandleRequestOperatorAcknowledgementSendCodeTwiceTest()
        {
            DoSetups(true);
            _aftRegistrationProvider.Setup(x => x.AftRegistrationStatus).Returns(AftRegistrationStatus.RegistrationPending);
            data.RegistrationCode = AftRegistrationCode.RequestOperatorAcknowledgement;
            data.AssetNumber = AssetNumber;
            data.RegistrationKey = validRegistrationKey;
            data.PosId = PosId;
            _target.Handle(data);
            _aftRegistrationProvider.Verify(x => x.ProcessAftRegistration(AftRegistrationCode.RequestOperatorAcknowledgement, data.AssetNumber, data.RegistrationKey, data.PosId), Times.Exactly(2));
        }

        private void DoSetups(bool initialized)
        {
            if (initialized)
            {
                _aftRegistrationProvider.Setup(x => x.IsAftRegistered).Returns(true);
                _aftRegistrationProvider.Setup(x => x.AftRegistrationStatus).Returns(AftRegistrationStatus.Registered);
                _aftRegistrationProvider.Setup(x => x.AftRegistrationKey).Returns(validRegistrationKey);
                _aftRegistrationProvider.Setup(x => x.PosId).Returns(PosId);
                _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(AssetNumber);
            }
            else
            {
                _aftRegistrationProvider.Setup(x => x.IsAftRegistered).Returns(false);
                _aftRegistrationProvider.Setup(x => x.AftRegistrationStatus).Returns(AftRegistrationStatus.NotRegistered);
                _aftRegistrationProvider.Setup(x => x.AftRegistrationKey).Returns(zeroRegistrationKey);
                _aftRegistrationProvider.Setup(x => x.PosId).Returns((uint)0);
                _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns((uint)0);
            }
        }
    }
}
