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
    using G2S.Handlers.Printer;
    using G2S.Services;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.TicketContent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetPrinterProfileTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetPrinterProfile(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullRegistryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetPrinterProfile(egm.Object, null, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullPrintLogsExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var handler = new GetPrinterProfile(egm.Object, registry.Object, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(egm.Object, registry.Object, logs.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(egm.Object, registry.Object, logs.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(
                HandlerUtilities.CreateMockEgm<IPrinterDevice>(),
                registry.Object,
                logs.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IPrinterDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(
                HandlerUtilities.CreateMockEgm<IPrinterDevice>(),
                registry.Object,
                logs.Object);

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

            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(egm, registry.Object, logs.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IPrinterDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            var handler = new GetPrinterProfile(HandlerUtilities.CreateMockEgm(device), registry.Object, logs.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var device = new Mock<IPrinterDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<printer, getPrinterProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var printer = new Mock<IPrinter>();
            var registry = new Mock<IDeviceRegistryService>();
            var logs = new Mock<IPrintLog>();
            registry.Setup(r => r.GetDevice<IPrinter>()).Returns(printer.Object);
            printer.SetupGet(p => p.Regions).Returns(Enumerable.Empty<PrintableRegion>());
            printer.SetupGet(p => p.Templates).Returns(Enumerable.Empty<PrintableTemplate>());

            logs.SetupGet(l => l.MinimumLogEntries).Returns(Constants.DefaultMinLogEntries);

            var handler = new GetPrinterProfile(HandlerUtilities.CreateMockEgm(device), registry.Object, logs.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<printer, printerProfile>;

            Assert.IsNotNull(response);
        }
    }
}