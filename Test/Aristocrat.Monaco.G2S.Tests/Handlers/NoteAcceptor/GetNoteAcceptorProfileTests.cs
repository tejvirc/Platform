namespace Aristocrat.Monaco.G2S.Tests.Handlers.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Kernel.Contracts;
    using G2S.Handlers.NoteAcceptor;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetNoteAcceptorProfileTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetNoteAcceptorProfile(null, null, null, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullDeviceRegistryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetNoteAcceptorProfile(egm.Object, null, null, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullTransactionHistoryProviderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var handler = new GetNoteAcceptorProfile(egm.Object, registry.Object, null, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var handler = new GetNoteAcceptorProfile(egm.Object, registry.Object, transaction.Object, null);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var properties = new Mock<IPropertiesManager>();
            var handler = new GetNoteAcceptorProfile(
                egm.Object,
                registry.Object,
                transaction.Object,
                properties.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var properties = new Mock<IPropertiesManager>();
            var handler = new GetNoteAcceptorProfile(
                egm.Object,
                registry.Object,
                transaction.Object,
                properties.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenHandleWithNoDeviceExpectSuccess()
        {
            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var properties = new Mock<IPropertiesManager>();
            var device = mock.Object;

            const int minLogEntries = 99;

            transaction.Setup(t => t.GetMaxTransactions<BillTransaction>()).Returns(minLogEntries);

            var handler = new GetNoteAcceptorProfile(egm, registry.Object, transaction.Object, properties.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorProfile>;

            Assert.IsNotNull(response);
            Assert.AreEqual(device.ConfigurationId, response.Command.configurationId);
            Assert.AreEqual(device.RestartStatus, response.Command.restartStatus);
            Assert.AreEqual(device.UseDefaultConfig, response.Command.useDefaultConfig);
            Assert.AreEqual(device.RequiredForPlay, response.Command.requiredForPlay);
            Assert.AreEqual(minLogEntries, response.Command.minLogEntries);
            Assert.IsFalse(response.Command.noteEnabled);
            Assert.IsFalse(response.Command.voucherEnabled);
            Assert.AreEqual(device.ConfigDateTime, response.Command.configDateTime);
            Assert.AreEqual(device.ConfigComplete, response.Command.configComplete);
            Assert.AreEqual(t_g2sBoolean.G2S_false, response.Command.promoSupported);
        }

        [TestMethod]
        public async Task WhenHandleWithNoDenominationsEnabledExpectSuccess()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var noteAcceptor = new Mock<INoteAcceptor>();
            var properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);

            const int minLogEntries = 99;

            transaction.Setup(t => t.GetMaxTransactions<BillTransaction>()).Returns(minLogEntries);
            registry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);
            noteAcceptor.SetupGet(n => n.Denominations).Returns(new List<int>( ));
            noteAcceptor.Setup(n => n.GetSupportedNotes(It.IsAny<string>())).Returns(new Collection<int> { 1, 5, 10, 20, 50, 100 });
            properties.Setup(
                m => m.GetProperty(PropertyKey.VoucherIn, false)).Returns(false);
            properties.Setup(
                    m => m.GetProperty(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId))
                .Returns(ApplicationConstants.DefaultCurrencyId);
            properties.Setup(
                    m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier))
                .Returns(ApplicationConstants.DefaultCurrencyMultiplier);

            var handler = new GetNoteAcceptorProfile(egm, registry.Object, transaction.Object, properties.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorProfile>;

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Command.noteEnabled);
            Assert.IsFalse(response.Command.voucherEnabled);
            Assert.IsTrue(response.Command.noteAcceptorData.Length > 0);
        }

        [TestMethod]
        public async Task WhenHandleWithDenominationsEnabledExpectSuccess()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            var mock = CreateNoteAcceptorDevice();
            var egm = HandlerUtilities.CreateMockEgm(mock);
            var registry = new Mock<IDeviceRegistryService>();
            var transaction = new Mock<ITransactionHistory>();
            var noteAcceptor = new Mock<INoteAcceptor>();
            var properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);

            const int minLogEntries = 99;

            transaction.Setup(t => t.GetMaxTransactions<BillTransaction>()).Returns(minLogEntries);
            registry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);
            noteAcceptor.SetupGet(n => n.Denominations).Returns(new List<int>{1, 5, 10, 20, 50, 100});
            noteAcceptor.Setup(n => n.GetSupportedNotes(It.IsAny<string>())).Returns(new Collection<int> { 1, 5, 10, 20, 50, 100 });
            properties.Setup(
                    m => m.GetProperty(PropertyKey.VoucherIn, false)).Returns(true);
            properties.Setup(
                    m => m.GetProperty(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId))
                .Returns(ApplicationConstants.DefaultCurrencyId);
            properties.Setup(
                    m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier))
                .Returns(ApplicationConstants.DefaultCurrencyMultiplier);
            var handler = new GetNoteAcceptorProfile(egm, registry.Object, transaction.Object, properties.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorProfile>;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Command.noteEnabled);
            Assert.IsTrue(response.Command.voucherEnabled);
            Assert.IsTrue(response.Command.noteAcceptorData.Length > 0);
        }

        private static Mock<INoteAcceptorDevice> CreateNoteAcceptorDevice()
        {
            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(m => m.Owner).Returns(123);
            device.SetupGet(m => m.Guests).Returns(new[] { TestConstants.HostId });
            device.SetupGet(m => m.Active).Returns(true);
            device.SetupGet(m => m.ConfigurationId).Returns(1);
            device.SetupGet(m => m.RestartStatus).Returns(true);
            device.SetupGet(m => m.UseDefaultConfig).Returns(true);
            device.SetupGet(m => m.RequiredForPlay).Returns(true);
            device.SetupGet(m => m.ConfigDateTime).Returns(DateTime.MaxValue);
            device.SetupGet(m => m.ConfigComplete).Returns(true);

            return device;
        }
    }
}
