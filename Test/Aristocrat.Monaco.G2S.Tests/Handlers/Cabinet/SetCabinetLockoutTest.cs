namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetCabinetLockoutTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetCabinetLockout(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetCabinetLockout(egm.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var state = new Mock<IEgmStateManager>();

            var handler = new SetCabinetLockout(egm.Object, state.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var handler = new SetCabinetLockout(egm.Object, state.Object, command.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();

            var handler = new SetCabinetLockout(egm.Object, state.Object, command.Object, lift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(egm.Object, state.Object, command.Object, lift.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm<ICabinetDevice>(),
                state.Object,
                command.Object,
                lift.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICabinetDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(egm, state.Object, command.Object, lift.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithEgmDisabledExpectError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Enabled).Returns(false);

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            var lockOut = ClassCommandUtilities.CreateClassCommand<cabinet, setCabinetLockOut>(
                TestConstants.HostId,
                TestConstants.EgmId);

            lockOut.Command.lockOut = true;

            var result = await handler.Verify(lockOut);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.Code, ErrorCode.G2S_APX006);
        }

        [TestMethod]
        public async Task WhenVerifyWithHostDisabledExpectError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Enabled).Returns(true);
            device.SetupGet(d => d.HostEnabled).Returns(false);

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            var lockOut = ClassCommandUtilities.CreateClassCommand<cabinet, setCabinetLockOut>(
                TestConstants.HostId,
                TestConstants.EgmId);

            lockOut.Command.lockOut = true;

            var result = await handler.Verify(lockOut);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.Code, ErrorCode.G2S_APX006);
        }

        [TestMethod]
        public async Task WhenHandleWithLockoutExpectLock()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupAllProperties();
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            var lockOut = ClassCommandUtilities.CreateClassCommand<cabinet, setCabinetLockOut>(
                TestConstants.HostId,
                TestConstants.EgmId);

            lockOut.Command.lockOut = true;
            lockOut.Command.lockText = "Test lockout";
            lockOut.Command.lockTimeOut = 5000;

            await handler.Handle(lockOut);

            var response = lockOut.Responses.FirstOrDefault() as ClassCommand<cabinet, cabinetStatus>;

            Assert.IsNotNull(response);

            state.Verify(
                s => s.Lock(
                    device.Object,
                    EgmState.HostLocked,
                    It.Is<Func<string>>(x => x.Invoke() == lockOut.Command.lockText),
                    TimeSpan.FromMilliseconds(lockOut.Command.lockTimeOut),
                    It.IsAny<Action>()));

            lift.Verify(l => l.Report(device.Object, EventCode.G2S_CBE009, It.IsAny<deviceList1>()));
        }

        [TestMethod]
        public async Task WhenHandleWithUnlockExpectEnable()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupAllProperties();
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var state = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var handler = new SetCabinetLockout(
                HandlerUtilities.CreateMockEgm(device),
                state.Object,
                command.Object,
                lift.Object);

            var lockOut = ClassCommandUtilities.CreateClassCommand<cabinet, setCabinetLockOut>(
                TestConstants.HostId,
                TestConstants.EgmId);

            lockOut.Command.lockOut = false;

            await handler.Handle(lockOut);

            var response = lockOut.Responses.FirstOrDefault() as ClassCommand<cabinet, cabinetStatus>;

            Assert.IsNotNull(response);

            state.Verify(s => s.Enable(device.Object, EgmState.HostLocked));

            lift.Verify(l => l.Report(device.Object, EventCode.G2S_CBE010, It.IsAny<deviceList1>()));
        }
    }
}
