namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Drm;
    using Aristocrat.Monaco.Gaming.Contracts.Configuration;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.PackageManifest;
    using Aristocrat.Monaco.PackageManifest.Models;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Gaming.Runtime;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameProviderTests
    {
        private const int GameId = 15;
        private const string gsaManifestPath = "test_game\\";
        private const string gsaManifestFile = gsaManifestPath + "test_empty.gsamanifest";

        private GameProvider _target;
        private GameContent _gameContent;

        private Mock<IPathMapper> _pathMapper;
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IManifest<GameContent>> _manifest;
        private Mock<IGameMeterManager> _meters;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IGameOrderSettings> _gameOrder;
        private Mock<IEventBus> _bus;
        private Mock<IPropertiesManager> _properties;
        private Mock<IRuntimeProvider> _runtimeProvider;
        private Mock<IManifest<IEnumerable<ProgressiveDetail>>> _progressiveManifest;
        private Mock<IProgressiveLevelProvider> _progressiveProvider;
        private Mock<IIdProvider> _idProvider;
        private Mock<IDigitalRights> _digitalRights;
        private Mock<IConfigurationProvider> _configurationProvider;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;
        private Mock<IServiceManager> _serviceManager;

        [TestInitialize]
        public void Initialize()
        {
            SetupInjections();
            SetupProperties();
            SetupStorage();
            SetupFiles();
            SetupDetails();
            SetupServiceManager();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            CreateTarget();

            Assert.IsNotNull(_target);
        }

        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true)]
        public void WhenArgumentIsNullExpectException(
            bool nullPath,
            bool nullStorage,
            bool nullManifest,
            bool nullMeters,
            bool nullDisable,
            bool nullGameOrder,
            bool nullBus,
            bool nullProperties,
            bool nullRuntime,
            bool nullProgressiveManifest,
            bool nullProgressiveProvider,
            bool nullId,
            bool nullDigitalRights,
            bool nullConfiguration,
            bool nullCabinet)
        {
            CreateTarget(
                nullPath,
                nullStorage,
                nullManifest,
                nullMeters,
                nullDisable,
                nullGameOrder,
                nullBus,
                nullProperties,
                nullRuntime,
                nullProgressiveManifest,
                nullProgressiveProvider,
                nullId,
                nullDigitalRights,
                nullConfiguration,
                nullCabinet);
        }

        private void CreateTarget(
            bool nullPath = false,
            bool nullStorage = false,
            bool nullManifest = false,
            bool nullMeters = false,
            bool nullDisable = false,
            bool nullGameOrder = false,
            bool nullBus = false,
            bool nullProperties = false,
            bool nullRuntime = false,
            bool nullProgressiveManifest = false,
            bool nullProgressiveProvider = false,
            bool nullId = false,
            bool nullDigitalRights = false,
            bool nullConfiguration = false,
            bool nullCabinet = false)
        {
            _target = new GameProvider(
                nullPath ? null : _pathMapper.Object,
                nullStorage ? null : _storageManager.Object,
                nullManifest ? null : _manifest.Object,
                nullMeters ? null : _meters.Object,
                nullDisable ? null : _disableManager.Object,
                nullGameOrder ? null : _gameOrder.Object,
                nullBus ? null : _bus.Object,
                nullProperties ? null : _properties.Object,
                nullRuntime ? null : _runtimeProvider.Object,
                nullProgressiveManifest ? null : _progressiveManifest.Object,
                nullProgressiveProvider ? null : _progressiveProvider.Object,
                nullId ? null : _idProvider.Object,
                nullDigitalRights ? null : _digitalRights.Object,
                nullConfiguration ? null : _configurationProvider.Object,
                nullCabinet ? null : _cabinetDetectionService.Object);

            _target.Initialize();
        }

        private void SetupInjections()
        {
            _pathMapper = new Mock<IPathMapper>(MockBehavior.Default);
            _storageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _manifest = new Mock<IManifest<GameContent>>(MockBehavior.Default);
            _meters = new Mock<IGameMeterManager>(MockBehavior.Default);
            _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameOrder = new Mock<IGameOrderSettings>(MockBehavior.Default);
            _bus = new Mock<IEventBus>(MockBehavior.Default);
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _runtimeProvider = new Mock<IRuntimeProvider>(MockBehavior.Default);
            _progressiveManifest = new Mock<IManifest<IEnumerable<ProgressiveDetail>>>(MockBehavior.Default);
            _progressiveProvider = new Mock<IProgressiveLevelProvider>(MockBehavior.Default);
            _idProvider = new Mock<IIdProvider>(MockBehavior.Default);
            _digitalRights = new Mock<IDigitalRights>(MockBehavior.Default);
            _configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Default);
            _cabinetDetectionService = new Mock<ICabinetDetectionService>(MockBehavior.Default);
        }

        private void SetupProperties()
        {
            _properties.Setup(p => p.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>())).Returns(1d);
            _properties.Setup(p => p.GetProperty(ApplicationConstants.DemonstrationMode, It.IsAny<bool>())).Returns(false);
            _properties.Setup(p => p.GetProperty(GamingConstants.AllowSlotGames, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, It.IsAny<bool>())).Returns(false);
            _properties.Setup(p => p.GetProperty(GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp, It.IsAny<bool>())).Returns(false);
            _properties.Setup(p => p.GetProperty(GamingConstants.SlotMinimumReturnToPlayer, It.IsAny<int>())).Returns(int.MinValue);
            _properties.Setup(p => p.GetProperty(GamingConstants.SlotMaximumReturnToPlayer, It.IsAny<int>())).Returns(int.MaxValue);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxBetLimit, It.IsAny<long>())).Returns(AccountingConstants.DefaultMaxBetLimit);
            _properties.Setup(p => p.GetProperty(GamingConstants.ServerControlledPaytables, It.IsAny<bool>())).Returns(false);
            _properties.Setup(p => p.GetProperty(GamingConstants.MaximumGameRoundWinAmount, It.IsAny<long>())).Returns(1000000L);
        }

        private void SetupStorage()
        {
            var block = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
            var storageTransaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);
            var scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);

            var results = new Dictionary<int, Dictionary<string, object>>
            {
                [0] = new Dictionary<string, object>
                {
                    ["Game.Denominations"] = "[{\"Id\":1,\"Value\":1000,\"Active\":true,\"PreviousActiveTime\":\"00:00:00\",\"ActiveDate\":\"2021-07-21T14:20:58.6187801Z\",\"MinimumWagerCredits\":75,\"BetOption\":\"1 to 6\",\"LineOption\":\"1024 Ways\",\"BonusBet\":0,\"SecondaryAllowed\":false}]",
                    ["Game.Id"] = 1,
                    ["Game.InstallDate"] = "",
                    ["Game.MaximumWagerCredits"] = 450,
                    ["Game.PaytableId"] = "ATI_SG0246-00_11",
                    ["Game.Status"] = 0,
                    ["Game.Tags"] = "",
                    ["Game.ThemeId"] = "ATI_BuffaloGoldRevolution",
                    ["Game.ThemeName"] = "BuffaloGoldRevolution",
                    ["Game.Upgraded"] = true,
                    ["Game.Version"] = "1.01 - 67488",
                    ["Game.WagerCategories"] = "[{ \"Id\":\"1\",\"TheoPaybackPercent\":90.0,\"MinWagerCredits\":75,\"MaxWagerCredits\":75,\"MaxWinAmount\":0},{ \"Id\":\"2\",\"TheoPaybackPercent\":90.0,\"MinWagerCredits\":150,\"MaxWagerCredits\":150,\"MaxWinAmount\":0},{ \"Id\":\"3\",\"TheoPaybackPercent\":90.0,\"MinWagerCredits\":225,\"MaxWagerCredits\":225,\"MaxWinAmount\":0},{ \"Id\":\"4\",\"TheoPaybackPercent\":90.0,\"MinWagerCredits\":300,\"MaxWagerCredits\":300,\"MaxWinAmount\":0},{ \"Id\":\"5\",\"TheoPaybackPercent\":90.0,\"MinWagerCredits\":450,\"MaxWagerCredits\":450,\"MaxWinAmount\":0}]",
                    ["Game.Category"] = 0,
                    ["Game.SubCategory"] = 0,
                    ["Game.Features"] = "",
                    ["Game.CdsGameInfos"] = "",
                    ["Game.MinimumPaybackPercent"] = "90.222",
                    ["Game.MaximumPaybackPercent"] = "91.2222"
                }
            };

            block.Setup(b => b.GetAll()).Returns(results);
            block.Setup(m => m.StartTransaction()).Returns(storageTransaction.Object);

            scopedTransaction.Setup(x => x.Complete());

            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(block.Object);
            _storageManager.Setup(m => m.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>())).Returns(block.Object);
            _storageManager.Setup(x => x.ScopedTransaction()).Returns(scopedTransaction.Object);
        }

        private void SetupFiles()
        {
            _pathMapper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo("."));
            _gameContent = new GameContent();

            var attributes = new GameAttributes
            {
                CentralInfo = new List<CentralInfo>(),
                WagerCategories = new List<PackageManifest.Models.WagerCategory>() { new PackageManifest.Models.WagerCategory { MinWagerCredits = 1 } },
                Denominations = new List<long>() { 1 },
                PaytableId = "TestPaytableId",
            };

            _gameContent.GameAttributes = new List<GameAttributes> { attributes };
            _gameContent.ReleaseNumber = "1";
            _manifest.Setup(m => m.Read(It.IsAny<string>())).Returns(_gameContent);

            if (!File.Exists(gsaManifestFile))
            {
                Directory.CreateDirectory(gsaManifestPath);
                File.Create(gsaManifestFile);
            }
        }

        private void SetupDetails()
        {
            _disableManager.Setup(d => d.CurrentDisableKeys).Returns(new List<Guid>());
            _digitalRights.Setup(d => d.License.Configuration).Returns(Application.Contracts.Drm.Configuration.Vip);

            var detail = new ProgressiveDetail
            {
                Variation = "ALL",
                ReturnToPlayer = new PackageManifest.Models.ProgressiveRtp
                {
                    ResetRtpMin = 1.520M,
                    ResetRtpMax = 1.772M,
                    IncrementRtpMin = 1.375M,
                    IncrementRtpMax = 1.375M,
                    BaseRtpAndResetRtpMin = 1.520M,
                    BaseRtpAndResetRtpMax = 1.772M
                }
            };

            var details = new List<ProgressiveDetail>() { detail };
            _progressiveManifest.Setup(p => p.Read(It.IsAny<string>())).Returns(details);

            _idProvider.Setup(i => i.GetNextDeviceId<GameDetail>()).Returns(GameId);
        }

        private void SetupServiceManager()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _serviceManager.Setup(s => s.GetService<IEventBus>()).Returns(_bus.Object);
        }
    }
}
