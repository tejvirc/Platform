namespace Aristocrat.Monaco.Gaming.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Tickets;
    using Aristocrat.Monaco.Application.Contracts.Currency;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Gaming.Tickets;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PackageManifest.Models;
    using Test.Common;

    /// <summary>
    ///     Internal class for testing
    /// </summary>
    internal class TestGameProfile : IGameDetail
    {
        public string PositionPriorityKey { get; }

        public bool ProgressiveAllowed { get; set; }

        public bool SecondaryAllowed { get; set; }

        public string BetOption { get; set; }

        public string BonusBet { get; set; }

        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime InstallDate { get; set; }

        public bool Upgraded { get; set; }

        public int Id { get; set; }

        public string ThemeId { get; set; }

        public string PaytableId { get; set; }

        public bool Active { get; set; }

        public int MaximumWagerCredits { get; set; }

        public int MinimumWagerCredits { get; set; }

        public long MaximumWinAmount { get; set; }

        public bool CentralAllowed { get; set; }

        public decimal MaximumPaybackPercent { get; set; }

        public decimal MinimumPaybackPercent { get; set; }

        public string ThemeName { get; set; }

        public string PaytableName { get; set; }

        public string CdsThemeId { get; set; }

        public long? ProductCode { get; set; }

        public IEnumerable<long> ActiveDenominations { get; set; }

        public IEnumerable<long> SupportedDenominations => Denominations.Select(d => d.Value);

        public IEnumerable<IDenomination> Denominations { get; set; }

        public IEnumerable<IWagerCategory> WagerCategories { get; set; }

        public IEnumerable<IWinLevel> WinLevels { get; set; }

        public string VariationId { get; set; }

        public string ReferenceId { get; set; }

        public bool Enabled { get; set; }

        public bool EgmEnabled { get; set; }

        public GameStatus Status { get; set; }

        public IEnumerable<string> GameTags { get; set; }

        public BetOptionList BetOptionList { get; set; }

        public BetOption ActiveBetOption { get; }

        public LineOptionList LineOptionList { get; }

        public LineOption ActiveLineOption { get; }

        public BetLinePresetList BetLinePresetList { get; }

        public long WinThreshold { get; }

        public int? MaximumProgressivePerDenom { get; }

        public bool AutoPlaySupported { get; }

        public GameType GameType { get; set; }

        public string GameSubtype { get; set; }

        public string Folder { get; }

        public string GameDll { get; }

        public Dictionary<string, ILocaleGameGraphics> LocaleGraphics { get; }

        public string DisplayMeterName { get; }

        public IEnumerable<string> AssociatedSapDisplayMeterName { get; set; }

        public GameIconType GameIconType { get; set; }

        public long InitialValue { get; }

        public string TargetRuntime { get; }

        public GameCategory Category { get; }

        public GameSubCategory SubCategory { get; }

        public IEnumerable<Feature> Features { get; set; }

        public int MechanicalReels { get; set; }

        public int[] MechanicalReelHomeStops { get; set; }
    }

    internal class TestDenomination : IDenomination
    {
        public TestDenomination(long id, long value, bool active)
        {
            Id = id;
            Value = value;
            Active = active;
        }

        public long Id { get; }

        public long Value { get; }

        public bool Active { get; }

        public TimeSpan PreviousActiveTime { get; set; }

        public DateTime ActiveDate { get; set; }

        public int MinimumWagerCredits { get; set; }

        public int MaximumWagerCredits { get; set; }

        public int MaximumWagerOutsideCredits { get; set; }

        public string BetOption { get; set; }

        public string LineOption { get; set; }

        public int BonusBet { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool SecondaryEnabled { get; set; }

        public bool LetItRideAllowed { get; set; }

        public bool LetItRideEnabled { get; set; }
    }

    /// <summary>
    ///     Summary description for SingaporeClubsAuditTicketCreatorTest
    /// </summary>
    [TestClass]
    public class SingaporeClubsAuditTicketCreatorTest
    {
        private readonly IReadOnlyCollection<IGameDetail> _mockGames = new List<IGameDetail>
        {
            new TestGameProfile
            {
                Id = 1,
                Enabled = true,
                EgmEnabled = true,
                ThemeName = "Game1",
                Denominations = new List<IDenomination>
                {
                    new TestDenomination(1, 1000, true),
                    new TestDenomination(2, 5000, false),
                    new TestDenomination(3, 25000, false)
                }
            },
            new TestGameProfile
            {
                Id = 2,
                Enabled = false,
                EgmEnabled = false,
                ThemeName = "Game2",
                Denominations = new List<IDenomination>
                {
                    new TestDenomination(1, 1000, true),
                    new TestDenomination(2, 5000, false),
                    new TestDenomination(3, 25000, false)
                }
            },
            new TestGameProfile
            {
                Id = 3,
                Enabled = true,
                EgmEnabled = true,
                ThemeName = "Game3",
                Denominations = new List<IDenomination>
                {
                    new TestDenomination(1, 1000, true),
                    new TestDenomination(2, 5000, false),
                    new TestDenomination(3, 25000, true)
                }
            }
        };

        public const string dateTimeFormat = ApplicationConstants.DefaultDateTimeFormat;

        private readonly ProgressiveLevel _sampleProgressive = new ProgressiveLevel
        {
            DeviceId = 1,
            GameId = 1,
            Denomination = new List<long> { 1000 },
            ProgressiveId = 1,
            LevelId = 1,
            LevelType = ProgressiveLevelType.Selectable,
            LevelName = "It's a test",
            AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked, "TestLevel")
        };

        private readonly List<JackpotTransaction> _mockTransactions;

        // Mock Services
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IGameProvider> _gameProvider;
        private Mock<ITransactionHistory> _transactions;
        private Mock<IProgressiveLevelProvider> _progressives;
        private Mock<ITime> _time;
        private Mock<IPrinter> _printerMock;

        // Test target
        private SingaporeClubsAuditTicketCreator _target;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        public SingaporeClubsAuditTicketCreatorTest()
        {
            _mockTransactions = Enumerable.Range(1, 11).Select(
                index =>
                    new JackpotTransaction(
                        1,
                        DateTime.UtcNow.AddMinutes(index),
                        1,
                        1,
                        1,
                        1000,
                        1,
                        index * 1M.DollarsToMillicents(),
                        string.Empty,
                        1,
                        1M.DollarsToMillicents(),
                        (int)ProgressiveLevelType.Sap,
                        string.Empty,
                        PayMethod.Any,
                        0,
                        0)
                    {
                        TransactionId = index,
                        LogSequence = index,
                        PaidAmount = index * 1M.DollarsToMillicents()
                    }).ToList();
        }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Strict);
            _transactions = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Strict);
            _progressives = MoqServiceManager.CreateAndAddService<IProgressiveLevelProvider>(MockBehavior.Loose);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printerMock.Setup(mock => mock.GetCharactersPerLine(false, 0)).Returns(36);

            _target = new SingaporeClubsAuditTicketCreator();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreNotEqual(null, _target);
        }

        [TestMethod]
        public void InitializeTest()
        {
            _target.Initialize();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Singapore Clubs Audit Ticket", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedType = typeof(ISingaporeClubsAuditTicketCreator);
            var actualTypes = _target.ServiceTypes;
            Assert.AreEqual(1, actualTypes.Count);
            Assert.AreEqual(expectedType, actualTypes.First());
        }

        [TestMethod]
        public void TicketCreationAndContentTest()
        {
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture);


            // Mock properties
            var serialNumber = "123";
            uint machineId = 99;
            var sysVersion = "1.0.0.0";
            var now = _time.Object.GetLocationTime(DateTime.UtcNow);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, It.IsAny<object>()))
                .Returns(machineId);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<object>()))
                .Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>()))
                .Returns(sysVersion);
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<object>()))
                .Returns(10000000000000);

            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(_mockGames);

            _meterManager.SetupGet(m => m.GetMeter(It.IsAny<string>()).Classification)
                .Returns(new CurrencyMeterClassification());
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.SetupGet(m => m.GetMeter(It.IsAny<string>()).Lifetime).Returns(1);

            _transactions.Setup(m => m.RecallTransactions<JackpotTransaction>()).Returns(_mockTransactions);

            _progressives.Setup(m => m.GetProgressiveLevels()).Returns(new List<ProgressiveLevel> { _sampleProgressive });

            // Call target method
            var tickets = _target.Create().ToList();

            // There should be exactly 2 tickets
            Assert.AreEqual(tickets.Count, 2);

            // --- Check first ticket

            var ticket1LeftCol = tickets[0]["left"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            ticket1LeftCol = ticket1LeftCol.Where(entry => entry != string.Empty).ToList();

            // The left column is the field labels, there should always be 30 non-empty entries
            Assert.AreEqual(ticket1LeftCol.Count, 30);

            //var ticket1CenterCol = tickets[0]["center"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            //    .ToList();
            //ticket1CenterCol = ticket1CenterCol.Where(entry => entry != string.Empty).ToList();

            // The center column is the header values, there should always be 9 non-empty entries
            //Assert.AreEqual(ticket1CenterCol.Count, 9);

            var ticket1RightCol = tickets[0]["right"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            ticket1RightCol = ticket1RightCol.Where(entry => entry != string.Empty).ToList();

            // Expected Ticket fields where the field value is checkable.
            var verifiableFields = new Dictionary<string, string>
            {
                { "DATE:", now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern) },
                { "TIME:", now.ToString("HH:mm:ss") },
                { "Asset Number:", machineId.ToString() },
                { "Serial Number:", serialNumber },
                { "Platform ID:", sysVersion },
                { "Game ID:", "Multi-Game" },
                { "Game Name:", "Multi-Game" },
                { "Game Denomination:", "Multi-Denom" }
            };

            // Verify the ticket header fields
            for (var i = 0; i < verifiableFields.Count; i++)
            {
                var item = verifiableFields.ElementAt(i);
                Assert.AreEqual(ticket1LeftCol[i], item.Key);
                Assert.AreEqual(ticket1RightCol[i], item.Value);
            }

            // The right column is the field values, there should always be 18 non-empty entries
            Assert.AreEqual(ticket1RightCol.Count, 30);

            // --- Check second ticket

            var ticket2LeftCol = tickets[1]["left"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            ticket2LeftCol = ticket2LeftCol.Where(entry => entry != string.Empty).ToList();

            // The left column is the field labels, there should always be 18 non-empty entries
            Assert.AreEqual(ticket2LeftCol.Count, 18);

            var ticket2CenterCol = tickets[1]["center"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            ticket2CenterCol = ticket2CenterCol.Where(entry => entry != string.Empty).ToList();

            var ticket2RightCol = tickets[1]["right"].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .ToList();
            ticket2RightCol = ticket2RightCol.Where(entry => entry != string.Empty).ToList();

            // The "Last" level on the ticket should be #9 (most recent hit)
            var lastLevelOnTicket = ticket2CenterCol[9];
            Assert.AreEqual(@"It's a test", lastLevelOnTicket);

            // The "Last" win on the ticket should be #0 (most recent hit)
            var lastWinOnTicket = ticket2RightCol[8];
            Assert.AreEqual(@"$11.00", lastWinOnTicket);

            // The right column is the field values, there should always be 10 non-empty entries
            Assert.AreEqual(ticket2RightCol.Count, 18);
        }
    }
}