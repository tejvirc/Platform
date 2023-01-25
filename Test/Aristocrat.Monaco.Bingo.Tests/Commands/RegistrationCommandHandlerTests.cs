namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Common.Exceptions;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RegistrationCommandHandlerTests
    {
        private const string SerialNumber = "ABC123";
        private const uint MachineId = 1234;
        private const string Version = "12.12.12.12";

        private readonly Mock<IRegistrationService> _registrationServer = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<INetworkInformationProvider> _networkProvider = new(MockBehavior.Default);
        private RegistrationCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>())).Returns(SerialNumber);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>())).Returns(MachineId);
            _propertiesManager.Setup(x => x.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>())).Returns(Version);

            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullRegistration, bool nullProperties, bool nullNetworkProvider)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CreateTarget(nullRegistration, nullProperties, nullNetworkProvider));
        }

        [TestMethod]
        public async Task GoodResponseCodeHandleTest()
        {
            _networkProvider.Setup(x => x.GetPhysicalAddress()).Returns(PhysicalAddress.None);
            _registrationServer.Setup(x => x.RegisterClient(It.Is<RegistrationMessage>(
                m => string.Equals(m.MachineSerial, SerialNumber) &&
                string.Equals(m.MachineNumber, MachineId.ToString()) &&
                string.Equals(m.PlatformVersion, Version)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RegistrationResults(ResponseCode.Ok)))
                .Verifiable();
            await _target.Handle(new RegistrationCommand());
            _registrationServer.Verify();
        }

        [DataRow(ResponseCode.Rejected)]
        [DataRow(ResponseCode.Disconnected)]
        [DataTestMethod]
        [ExpectedException(typeof(RegistrationException))]
        public async Task InvalidResponseCodeHandleTest(ResponseCode code)
        {
            _networkProvider.Setup(x => x.GetPhysicalAddress()).Returns(PhysicalAddress.None);
            _registrationServer.Setup(x => x.RegisterClient(It.Is<RegistrationMessage>(
                m => string.Equals(m.MachineSerial, SerialNumber) &&
                string.Equals(m.MachineNumber, MachineId.ToString()) &&
                string.Equals(m.PlatformVersion, Version)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RegistrationResults(code)));
            await _target.Handle(new RegistrationCommand());
        }

        [TestMethod]
        [ExpectedException(typeof(RegistrationException))]
        public async Task RegisterThrowsTest()
        {
            _registrationServer.Setup(x => x.RegisterClient(It.Is<RegistrationMessage>(
                m => string.Equals(m.MachineSerial, SerialNumber) &&
                string.Equals(m.MachineNumber, MachineId.ToString()) &&
                string.Equals(m.PlatformVersion, Version)), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException());
            await _target.Handle(new RegistrationCommand());
        }

        private RegistrationCommandHandler CreateTarget(
            bool nullRegistration = false,
            bool nullProperties = false,
            bool nullNetworkProvider = false)
        {
            return new RegistrationCommandHandler(
                nullRegistration ? null : _registrationServer.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullNetworkProvider ? null : _networkProvider.Object);
        }
    }
}