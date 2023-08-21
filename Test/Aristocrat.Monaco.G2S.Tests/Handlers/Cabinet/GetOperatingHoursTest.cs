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
    public class GetOperatingHoursTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetOperatingHours(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetOperatingHours(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();

            var handler = new GetOperatingHours(egm.Object, properties.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var properties = new Mock<IPropertiesManager>();
            var handler = new GetOperatingHours(egm.Object, properties.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var properties = new Mock<IPropertiesManager>();
            var handler = new GetOperatingHours(HandlerUtilities.CreateMockEgm<ICabinetDevice>(), properties.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var properties = new Mock<IPropertiesManager>();
            var handler = new GetOperatingHours(HandlerUtilities.CreateMockEgm(device), properties.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICabinetDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var properties = new Mock<IPropertiesManager>();
            var handler = new GetOperatingHours(egm, properties.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var properties = new Mock<IPropertiesManager>();
            var handler = new GetOperatingHours(HandlerUtilities.CreateMockEgm(device), properties.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectOperatingHours()
        {
            var device = new Mock<ICabinetDevice>();
            var properties = new Mock<IPropertiesManager>();

            var operatingHours = GetSampleOperatingHours();

            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(operatingHours);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, getOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetOperatingHours(HandlerUtilities.CreateMockEgm(device), properties.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, operatingHoursList>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.operatingHours.Length, operatingHours.Count());
        }

        [TestMethod]
        public async Task WhenHandleFilteredCommandExpectFilteredOperatingHours()
        {
            var device = new Mock<ICabinetDevice>();
            var properties = new Mock<IPropertiesManager>();

            var operatingHours = GetSampleOperatingHours();

            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(operatingHours);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, getOperatingHours>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.weekday = t_weekday.GTK_friday;

            var handler = new GetOperatingHours(HandlerUtilities.CreateMockEgm(device), properties.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, operatingHoursList>;

            Assert.IsNotNull(response);
            Assert.AreEqual(
                response.Command.operatingHours.Length,
                operatingHours.Count(h => h.Day == DayOfWeek.Friday));
        }

        private static IEnumerable<OperatingHours> GetSampleOperatingHours()
        {
            var operatingHours = new List<OperatingHours>();

            // Game play allowed between 8:00AM and 11:00PM
            operatingHours.AddRange(
                Enumerable.Range(0, 7)
                    .Select(
                        day =>
                            new OperatingHours
                            {
                                Day = (DayOfWeek)day,
                                Time = (int)TimeSpan.FromHours(8).TotalMilliseconds,
                                Enabled = true
                            }));

            operatingHours.AddRange(
                Enumerable.Range(0, 7)
                    .Select(
                        day =>
                            new OperatingHours
                            {
                                Day = (DayOfWeek)day,
                                Time = (int)TimeSpan.FromHours(23).TotalMilliseconds,
                                Enabled = true
                            }));

            return operatingHours;
        }
    }
}