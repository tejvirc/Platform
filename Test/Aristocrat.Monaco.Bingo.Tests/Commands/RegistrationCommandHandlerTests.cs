namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Bingo.Commands;
    using Aristocrat.Monaco.Bingo.Common.Exceptions;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RegistrationCommandHandlerTests
    {
        private const string SerialNumber = "ABC123";
        private const uint MachineId = 1234;
        private const string Version = "12.12.12.12";

        private readonly Mock<IRegistrationService> _registrationServer = new Mock<IRegistrationService>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        public RegistrationCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>())).Returns(SerialNumber);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>())).Returns(MachineId);
            _propertiesManager.Setup(x => x.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>())).Returns(Version);

            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullRegistration, bool nullProperties)
        {
            CreateTarget(nullRegistration, nullProperties);
        }

        [TestMethod]
        public async Task GoodResposeCodeHandleTest()
        {
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

        private RegistrationCommandHandler CreateTarget(bool nullRegistration = false, bool nullProperties = false)
        {
            return new RegistrationCommandHandler(
                nullRegistration ? null : _registrationServer.Object,
                nullProperties ? null : _propertiesManager.Object);
        }
    }
}