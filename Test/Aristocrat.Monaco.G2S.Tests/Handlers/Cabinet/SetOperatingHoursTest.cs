namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Operations;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Cabinet;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetOperatingHoursTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetOperatingHours(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetOperatingHours(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();

            var handler = new SetOperatingHours(egm.Object, properties.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();

            var handler = new SetOperatingHours(egm.Object, properties.Object, eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(egm.Object, properties.Object, eventLift.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm<ICabinetDevice>(),
                properties.Object,
                eventLift.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm(device),
                properties.Object,
                eventLift.Object);

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

            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(egm, properties.Object, eventLift.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidTimeExpectError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm(device),
                properties.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.operatingHours = new[]
            {
                new operatingHours { time = (int)TimeSpan.FromHours(25).TotalMilliseconds }
            };

            var result = await handler.Verify(command);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.Code, ErrorCode.GTK_CBX001);
        }

        [TestMethod]
        public async Task WhenVerifyWithConflictsExpectError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm(device),
                properties.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.operatingHours = new[]
            {
                new operatingHours
                {
                    time = (int)TimeSpan.FromHours(7).TotalMilliseconds,
                    weekday = t_weekday.GTK_friday,
                    state = t_operatingHoursState.GTK_enable
                },
                new operatingHours
                {
                    time = (int)TimeSpan.FromHours(7).TotalMilliseconds,
                    weekday = t_weekday.GTK_friday,
                    state = t_operatingHoursState.GTK_disable
                }
            };

            var result = await handler.Verify(command);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(result.Code, ErrorCode.GTK_CBX002);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm(device),
                properties.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.operatingHours = new operatingHours[0];

            var result = await handler.Verify(command);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectSet()
        {
            var device = new Mock<ICabinetDevice>();
            var properties = new Mock<IPropertiesManager>();
            var eventLift = new Mock<IEventLift>();

            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(Enumerable.Empty<OperatingHours>());

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.operatingHours = new[]
            {
                new operatingHours
                {
                    time = (int)TimeSpan.FromHours(7).TotalMilliseconds,
                    weekday = t_weekday.GTK_all,
                    state = t_operatingHoursState.GTK_enable
                },
                new operatingHours
                {
                    time = (int)TimeSpan.FromHours(23).TotalMilliseconds,
                    weekday = t_weekday.GTK_all,
                    state = t_operatingHoursState.GTK_disable
                }
            };

            var handler = new SetOperatingHours(
                HandlerUtilities.CreateMockEgm(device),
                properties.Object,
                eventLift.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, operatingHoursList>;

            Assert.IsNotNull(response);

            properties.Verify(
                p =>
                    p.SetProperty(
                        ApplicationConstants.OperatingHours,
                        It.IsAny<IEnumerable<OperatingHours>>()));

            eventLift.Verify(e => e.Report(device.Object, EventCode.GTK_CBE001));
        }
    }
}