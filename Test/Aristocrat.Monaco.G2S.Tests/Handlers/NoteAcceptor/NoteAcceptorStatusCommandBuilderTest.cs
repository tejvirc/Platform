namespace Aristocrat.Monaco.G2S.Tests.Handlers.NoteAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.NoteAcceptor;
    using Hardware.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NoteAcceptorStatusCommandBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceRegistryServiceExpectException()
        {
            var builder = new NoteAcceptorStatusCommandBuilder(null, null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var registry = new Mock<IDeviceRegistryService>();
            var doorService = new Mock<IDoorService>();

            var builder = new NoteAcceptorStatusCommandBuilder(registry.Object, doorService.Object);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public async Task WhenBuildExpectSuccess()
        {
            var noteAcceptor = new Mock<INoteAcceptor>();
            var deviceService = noteAcceptor.As<IDeviceService>();
            var registry = new Mock<IDeviceRegistryService>();
            var doorService = new Mock<IDoorService>();

            doorService.Setup(a => a.GetDoorOpen((int)DoorLogicalId.CashBox)).Returns(false);

            registry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);

            deviceService.SetupAllProperties();

            var builder = new NoteAcceptorStatusCommandBuilder(registry.Object, doorService.Object);

            var device = new Mock<INoteAcceptorDevice>();
            var status = new noteAcceptorStatus();

            await builder.Build(device.Object, status);

            Assert.AreEqual(device.Object.ConfigurationId, status.configurationId);
            Assert.AreEqual(device.Object.ConfigDateTime, status.configDateTime);
            Assert.AreEqual(device.Object.ConfigComplete, status.configComplete);

            Assert.AreEqual(device.Object.HostEnabled, status.hostEnabled);
            Assert.AreEqual(device.Object.Enabled, status.egmEnabled);

            Assert.AreEqual(
                noteAcceptor.Object.LogicalState == NoteAcceptorLogicalState.Uninitialized,
                status.disconnected);
            Assert.AreEqual(
                noteAcceptor.Object.StackerState == NoteAcceptorStackerState.Removed,
                status.stackerRemoved);
            Assert.AreEqual(noteAcceptor.Object.StackerState == NoteAcceptorStackerState.Full, status.stackerFull);
            Assert.AreEqual(noteAcceptor.Object.StackerState == NoteAcceptorStackerState.Jammed, status.stackerJam);
            Assert.AreEqual(noteAcceptor.Object.StackerState == NoteAcceptorStackerState.Fault, status.stackerFault);
        }
    }
}