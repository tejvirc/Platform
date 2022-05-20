namespace Aristocrat.Monaco.G2S.Tests.Handlers.Printer
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
    using G2S.Handlers.Printer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetPrinterStatusTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetPrinterStatus(null, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetPrinterStatus(egm.Object, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(egm.Object, command.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(egm.Object, command.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(HandlerUtilities.CreateMockEgm<IPrinterDevice>(), command.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IPrinterDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(HandlerUtilities.CreateMockEgm<IPrinterDevice>(), command.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IPrinterDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(egm, command.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IPrinterDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var command = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(HandlerUtilities.CreateMockEgm(device), command.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var device = new Mock<IPrinterDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<printer, getPrinterStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var builder = new Mock<ICommandBuilder<IPrinterDevice, printerStatus>>();
            var handler = new GetPrinterStatus(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<printer, printerStatus>;

            Assert.IsNotNull(response);

            builder.Verify(b => b.Build(device.Object, It.IsAny<printerStatus>()));
        }
    }
}