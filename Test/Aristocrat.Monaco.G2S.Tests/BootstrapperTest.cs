namespace Aristocrat.Monaco.G2S.Tests
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client;
    using CompositionRoot;
    using Data.Profile;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using System.IO;
    using System.Linq;
    using Application.Contracts.Localization;
    using Application.Contracts.Protocol;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Protocol.Common.Installer;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.TowerLight;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;
    using Constants = G2S.Constants;
    using Aristocrat.G2S.Client.Communications;

    [TestClass]
    public class BootstrapperTest
    {

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IMonacoContextFactory>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            var pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Default);
            pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo(Constants.DataPath));

            var storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            var storageAccessor = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(storageAccessor.Object);

            var properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            properties.Setup(m => m.GetProperty(Constants.Port, Constants.DefaultPort)).Returns(Constants.DefaultPort);
            properties.Setup(m => m.GetProperty(ApplicationConstants.DemonstrationMode, false)).Returns(false);
            properties.Setup(m => m.GetProperty(ApplicationConstants.HandpayReceiptPrintingEnabled, It.IsAny<bool>())).Returns(false);
            properties.Setup(m => m.GetProperty(G2S.Constants.G2SProgressivesEnabled, It.IsAny<bool>())).Returns(true);

            MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IHardMeter>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGameMeterManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ICabinetService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGamePlayState>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IDeviceRegistryService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGameHistory>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ILocalization>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGameInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPrinterFirmwareInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<INoteAcceptorFirmwareInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISoftwareInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPlayerBank>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IIdProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IDisableByOperatorManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IMediaProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IGameOrderSettings>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IDisplayService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ITransferOutHandler>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPlayerService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPlayerSessionHistory>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IAttendantService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IIdReaderProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IComponentRegistry>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IAuthenticationService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProfileService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProfileDataRepository>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IMediaPlayerResizeManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ICentralProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProtocolProgressiveEventsRegistry>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProgressiveLevelProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProgressiveMeterManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOSInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPrinterFirmwareInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<INoteAcceptorFirmwareInstaller>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IWcfApplicationRuntime>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Default);

            var persistence = MoqServiceManager.CreateAndAddService<IPersistenceProvider>(MockBehavior.Default);
            persistence.Setup(a => a.GetOrCreateBlock(It.IsAny<string>(), It.IsAny<PersistenceLevel>())).Returns(new Mock<IPersistentBlock>().Object);

            MoqServiceManager.CreateAndAddService<ITowerLight>(MockBehavior.Default);

            var bonusHandler = MoqServiceManager.CreateAndAddService<IBonusHandler>(MockBehavior.Default);
            bonusHandler.SetupGet(m => m.Transactions).Returns(Enumerable.Empty<BonusTransaction>().ToList());
        }

        [TestMethod]
        public void VerifyContainerTest()
        {
            var container = Bootstrapper.InitializeContainer();
            container.Verify();
        }

        [TestMethod]
        public void EgmConfigurationTest()
        {
            var container = Bootstrapper.InitializeContainer();
            var egm = (IG2SEgm)container.GetInstance(typeof(IG2SEgm));

            Assert.AreNotEqual(null, egm.Id);
            Assert.AreNotEqual(string.Empty, egm.Id);
            Assert.IsNotNull(egm.Address);
        }
    }
}
