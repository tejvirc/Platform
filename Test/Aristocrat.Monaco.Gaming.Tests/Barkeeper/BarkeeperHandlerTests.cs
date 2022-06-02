namespace Aristocrat.Monaco.Gaming.Tests.Barkeeper
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Common;
    using Contracts;
    using Contracts.Barkeeper;
    using Gaming.Barkeeper;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class BarkeeperHandlerTests
    {
        private const string BarkeeperRewardLevelsXml =
            "Aristocrat.Monaco.Gaming.Tests.Barkeeper.BarkeeperRewardLevels.xml";

        private const int SlowFlashTime = 500;
        private const int MediumFlashTime = 250;
        private const int FastFlashTime = 100;

        private static readonly Color CashInIdleColor = Color.White;
        private static readonly Color CashInActiveColor = Color.Blue;
        private static readonly Color CashInDisabledColor = Color.Black;
        private static readonly Color CoinInIdleColor = Color.Black;
        private static readonly Color CoinInDisabledColor = Color.Black;
        private static readonly Color CoinInLevel1Color = Color.Green;
        private static readonly Color CoinInLevel2Color = Color.Blue;
        private static readonly Color CoinInLevel3Color = Color.Orange;

        private static readonly List<int> ButtonLed = new List<int> { (int)StripIDs.BarkeeperStrip1Led };
        private static readonly List<int> HaloLed = new List<int> { (int)StripIDs.BarkeeperStrip4Led };

        private BarkeeperHandler _target;
        private Mock<IEdgeLightingController> _edgeLightController;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPlayerBank> _bank;
        private Mock<IGamePlayState> _gamePlayState;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _edgeLightController = new Mock<IEdgeLightingController>(MockBehavior.Default);
            _bank = new Mock<IPlayerBank>(MockBehavior.Default);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);

            var rewardLevelsXml = "";

            using (var stream = GetType().Assembly.GetManifestResourceStream(BarkeeperRewardLevelsXml))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        rewardLevelsXml = reader.ReadToEnd();
                    }
                }
            }

            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperRewardLevels, It.IsAny<string>()))
                .Returns(rewardLevelsXml);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperActiveCoinInReward, It.IsAny<string>()))
                .Returns(string.Empty);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCashIn, It.IsAny<long>()))
                 .Returns(0L);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCoinIn, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds, It.IsAny<long>()))
                .Returns(0L);

            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _target.Initialize();
            _edgeLightController.ResetCalls();
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEdgeLightControllerTest()
        {
            _target = new BarkeeperHandler(
                null,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                null,
                _gamePlayState.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGamePlayStateTest()
        {
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                null,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPlayerBankTest()
        {
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                null);
        }

        [DataRow(1000, 1000, true)]
        [DataRow(1000, 100, false)]
        [DataTestMethod]
        public void InitializeCreditTest(
            long thresholdAmount,
            long amount,
            bool lightsPlayed)
        {
            var creditReward = new RewardLevel
            {
                TriggerStrategy = BarkeeperStrategy.CashIn,
                Enabled = true,
                Alert = BarkeeperAlertOptions.LightOn,
                Color = ColorOptions.Blue,
                ThresholdInCents = thresholdAmount,
                Led = BarkeeperLed.Halo
            };

            var expectedColor = lightsPlayed ? CashInActiveColor : CashInIdleColor;
            var coinInRate = new CoinInRate { Enabled = false };

            SetupPersistence(
                amount.CentsToMillicents(),
                0L,
                new List<RewardLevel> { creditReward },
                new CoinInStrategy { CoinInRate = coinInRate },
                new CashInStrategy());
            _bank.Setup(x => x.Balance).Returns(amount.CentsToMillicents());
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.OnCreditsInserted(amount.CentsToMillicents());
            _target.Initialize();
            Verify(BarkeeperAlertOptions.LightOn, expectedColor, HaloLed, 1);
        }

        [DataRow(1000, 1000)]
        [DataRow(1000, 100)]
        [DataTestMethod]
        public void OnZeroBalanceTest(long thresholdAmount, long amount)
        {
            var creditReward = new RewardLevel
            {
                TriggerStrategy = BarkeeperStrategy.CashIn,
                Enabled = true,
                Alert = BarkeeperAlertOptions.LightOn,
                Color = ColorOptions.Blue,
                ThresholdInCents = thresholdAmount,
                Led = BarkeeperLed.Button
            };

            var coinInRate = new CoinInRate { Enabled = false };

            SetupPersistence(
                amount,
                0L,
                new List<RewardLevel> { creditReward },
                new CoinInStrategy { CoinInRate = coinInRate },
                new CashInStrategy());
            _bank.Setup(x => x.Balance).Returns(amount);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _target.Initialize();
            _target.OnCreditsInserted(amount);
            Assert.AreEqual(amount, _target.CreditsInDuringSession);

            _target.OnBalanceUpdate(0);
            Assert.AreEqual(0L, _target.CreditsInDuringSession);
        }

        [TestMethod]
        public void InitializeDisabledTest()
        {
            SetupPersistence(
                0L,
                0L,
                new List<RewardLevel>(),
                GetCoinInStrategy(),
                new CashInStrategy(),
                new RewardLevel());
            _edgeLightController.ResetCalls();
            _bank.Setup(x => x.Balance).Returns(10);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _target.Initialize();

            Verify(BarkeeperAlertOptions.LightOn, CashInDisabledColor, HaloLed, 1);
            Verify(BarkeeperAlertOptions.LightOn, CoinInDisabledColor, ButtonLed, 1);
        }

        [DataTestMethod]
        [DataRow(true, true, 1000, 1000000, true)]
        [DataRow(true, true, 1000, 100, false)]
        [DataRow(true, false, 1000, 1000000, false)]
        [DataRow(false, true, 1000, 1000000, false)]
        public void CreditTest(
            bool barkeeperEnabled,
            bool thresholdEnabled,
            long thresholdAmount,
            long creditAmount,
            bool lightsPlayed)
        {
            var cashInLevel = new RewardLevel
            {
                TriggerStrategy = BarkeeperStrategy.CashIn,
                Enabled = thresholdEnabled,
                ThresholdInCents = thresholdAmount,
                Led = BarkeeperLed.Halo,
                Color = ColorOptions.Yellow,
                Alert = BarkeeperAlertOptions.LightOn
            };

            SetupPersistence(
                creditAmount,
                0,
                new[] { cashInLevel },
                GetCoinInStrategy(),
                new CashInStrategy(),
                null,
                barkeeperEnabled);

            _target.OnCreditsInserted(creditAmount);
            Verify(BarkeeperAlertOptions.LightOn, Color.Yellow, HaloLed, lightsPlayed ? 1 : 0);
        }

        [DataTestMethod]
        [DataRow(true, true, 1000, 1000, true)]
        [DataRow(true, true, 1000, 100, false)]
        [DataRow(true, false, 1000, 1000, false)]
        [DataRow(false, true, 1000, 1000, false)]
        public void CreditsWageredTest(
            bool barkeeperEnabled,
            bool thresholdEnabled,
            long thresholdAmount,
            long creditAmount,
            bool lightsPlayed)
        {
            var coinInRewardLevel =
                new[]
                {
                    new RewardLevel
                    {
                        Enabled = thresholdEnabled,
                        ThresholdInCents = thresholdAmount,
                        Led = BarkeeperLed.Button,
                        Color = ColorOptions.Blue
                    }
                };
            SetupPersistence(
                0,
                creditAmount.CentsToMillicents(),
                coinInRewardLevel,
                GetCoinInStrategy(),
                new CashInStrategy(),
                null,
                barkeeperEnabled);
            _target.CreditsWagered(creditAmount.CentsToMillicents());
            Verify(BarkeeperAlertOptions.LightOn, Color.Blue, ButtonLed, lightsPlayed ? 1 : 0);
        }

        [TestMethod]
        public void TestCoinInLevels()
        {
            VerifyRewardLevel1();

            VerifyRewardLevel2();

            VerifyRewardLevel3();

            // Reconfigure anything i.e. Disable any level in coin-in reward and expect everything to reset.
            _target.RewardLevels.RewardLevels
                .First(x => x.TriggerStrategy == BarkeeperStrategy.CoinIn && x.Name.Equals("Level-3")).Enabled = false;
            _edgeLightController.ResetCalls();
            SetupPersistence(
                0,
                0,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy);
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);

            // Bet $2 and expect Level-2 to hit since Level-3 is disabled.
            _target.CreditsWagered(200000);
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 1);
        }

        [TestMethod]
        public void VerifyRewardLevelWithRateOfPlayTest()
        {

            VerifyRewardLevel1();

            VerifyRewardLevel2();

            Thread.Sleep(250);
            // Verify the Coin-In is reset to 0 as the session expires
            Assert.AreEqual(0, _target.CoinInDuringSession);

            // Bet $1 and expect Level-1 to hit
            _target.CreditsWagered(100000);
            Assert.AreEqual(100000, _target.CoinInDuringSession);
            // The last reward is retained (Barkeeper light is *NOT* switched to Level1 Color)
            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 0);
            _edgeLightController.ResetCalls();

            // Bet another $0.50 and expect Level-2 to hit
            _target.CreditsWagered(50000);
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            // The last reward is retained (Barkeeper light is *NOT* switched to Level2 Color)
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 0);
            _edgeLightController.ResetCalls();

            VerifyRewardLevel3();
        }

        [TestMethod]
        public void VerifyRateOfPlayWithPowerCycleInRewardLevelTest()
        {
            VerifyRewardLevel1();

            VerifyRewardLevel2();

            _propertiesManager.ResetCalls();
            Thread.Sleep(250);
            // Verify the Coin-In is reset to 0 as the session expires
            Assert.AreEqual(0, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[1], false);

            RewardLevel lastRewardLevel = _target.RewardLevels.RewardLevels[1].DeepClone();
            lastRewardLevel.Awarded = false;
            SetupPersistence(
                500000,
                0,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                lastRewardLevel);

            // Power cycle
            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.Initialize();

            // Verify Level-2 is triggered
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 1);

            // Bet $1 and expect Level-1 to hit
            _target.CreditsWagered(100000);
            Assert.AreEqual(100000, _target.CoinInDuringSession);
            // The last reward is retained (Barkeeper light is *NOT* switched to Level1 Color)
            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 0);
            _edgeLightController.ResetCalls();

            // Bet another $0.50 and expect Level-2 to hit
            _target.CreditsWagered(50000);
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            // The last reward is retained (Barkeeper light is *NOT* switched to Level2 Color)
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 0);
            _edgeLightController.ResetCalls();

            VerifyRewardLevel3();
            _propertiesManager.ResetCalls();
            Thread.Sleep(250);
            // CoinIn Session TimerElapsed
            Assert.AreEqual(0, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[2], false);
            VerifyRateOfPlayIsPersistedAsZeroOnce();
        }

        [TestMethod]
        public void VerifyBarkeeperButtonPressResetAwardLevel()
        {
            // Disable ROP as we are testing Barkeeper button press only
            _target.RewardLevels.CoinInStrategy.CoinInRate.Enabled = false;

            VerifyRewardLevel1();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will not reset Coin-In amount if AwardedLevel is not the top most level
            Assert.AreEqual(100000, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[0]);

            VerifyRewardLevel2();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will not reset Coin-In amount if AwardedLevel is not the top most level
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[1]);
            VerifyAwardedFlagsAreFalse();

            VerifyRewardLevel3();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            _edgeLightController.ResetCalls();
            VerifyAwardedFlagsAreFalse();
            // Pressing Barkeeper button will reset Coin-In amount to 0 as the AwardedLevel is the top most level
            Assert.AreEqual(0, _target.CoinInDuringSession);

            VerifyRewardLevel1();
        }

        [TestMethod]
        public void VerifyBarkeeperButtonPressWithPowerCycleResetAwardLevel()
        {
            // Disable ROP as we are testing Barkeeper button press only
            _target.RewardLevels.CoinInStrategy.CoinInRate.Enabled = false;

            VerifyRewardLevel1();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will not reset Coin-In amount if AwardedLevel is not the top most level
            Assert.AreEqual(100000, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[0]);

            VerifyRewardLevel2();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will not reset Coin-In amount if AwardedLevel is not the top most level
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[1]);
            VerifyAwardedFlagsAreFalse();

            RewardLevel lastARewardLevel = _target.RewardLevels.RewardLevels[1].DeepClone();
            lastARewardLevel.Awarded = true;
            SetupPersistence(
                500000,
                _target.CoinInDuringSession,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                lastARewardLevel);

            // Power cycle
            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.Initialize();

            // Verify Level-2 is not triggered
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 0);
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);

            VerifyRewardLevel3();

            // Bet $1 and expect *NO* change as we are in Level-3 already
            _target.CreditsWagered(100000);
            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 0);
            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);

            VerifyRewardLevel1();

            VerifyRewardLevel2();

            _propertiesManager.ResetCalls();
            VerifyRewardLevel3();
            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will reset Coin-In amount to 0 as the AwardedLevel is the top most level
            Assert.AreEqual(0, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistedAsNull();
            VerifyRateOfPlayIsPersistedAsZeroOnce();
        }

        [TestMethod]
        public void BarkeeperButtonPressAfterLevel2ResetOnlyLightTest()
        {
            // Disable ROP as we are testing Barkeeper button press only
            _target.RewardLevels.CoinInStrategy.CoinInRate.Enabled = false;

            VerifyRewardLevel1();

            VerifyRewardLevel2();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will not reset Coin-In amount if AwardedLevel is not the top most level
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[1]);
            VerifyAwardedFlagsAreFalse();

            VerifyRewardLevel3();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            // Pressing Barkeeper button will reset Coin-In amount to 0 as the AwardedLevel is the top most level
            Assert.AreEqual(0, _target.CoinInDuringSession);
            VerifyAwardedFlagsAreFalse();
        }

        [TestMethod]
        public void BarkeeperButtonPressShouldNotResetCashInAward()
        {
            _bank.Setup(x => x.Balance).Returns(100000);
            _target.OnCreditsInserted(100000);
            Verify(BarkeeperAlertOptions.LightOn, CashInActiveColor, HaloLed, 1);

            _target.BarkeeperButtonPressed();
            // Idle should not have called for CashInIdleColor
            Verify(BarkeeperAlertOptions.LightOn, CashInIdleColor, HaloLed, 0);
        }

        [TestMethod]
        public void SessionRestoreTest()
        {
            VerifyRewardLevel1();

            SetupPersistence(
                0,
                0,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                _target.RewardLevels.RewardLevels[0]);

            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.Initialize();

            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 1);
        }

        [TestMethod]
        public void AfterBarkeeperButtonPressSessionRestoredCorrectlyTest()
        {
            VerifyRewardLevel1();

            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);

            RewardLevel lastRewardLevel =  _target.RewardLevels.RewardLevels[0].DeepClone();
            lastRewardLevel.Awarded = true;
            SetupPersistence(
                0,
                0,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                lastRewardLevel);

            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.Initialize();

            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 0);
        }

        [TestMethod]
        public void SessionRestoreAndContinueToNextLevelTest()
        {
            _target.RewardLevels.CoinInStrategy.CoinInRate.SessionRateInMs = 100;

            VerifyRewardLevel1();

            VerifyRewardLevel2();

            RewardLevel lastRewardLevel = _target.RewardLevels.RewardLevels[1].DeepClone();
            SetupPersistence(
                500000,
                _target.CoinInDuringSession,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                lastRewardLevel
                );

            // Power cycle
            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _edgeLightController.ResetCalls();
            _target.Initialize();

            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, 1);
            Thread.Sleep(200);

            // Session Time-Out resets
            Assert.AreEqual(0, _target.CoinInDuringSession);

            _propertiesManager.ResetCalls();
            _edgeLightController.ResetCalls();
            _target.BarkeeperButtonPressed();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            _edgeLightController.ResetCalls();
            VerifyActiveCoinInRewardPersistence(_target.RewardLevels.RewardLevels[1]);

            VerifyRewardLevel1();

            VerifyRewardLevel2();

            VerifyRewardLevel3();
        }


        [TestMethod]
        public void GameEndedWithZeroCreditRemaining()
        {
            // Expect Coin-In timer to begin and expect CoinLevel to reset after 50ms.
            _target.CreditsWagered(200000);
            Verify(BarkeeperAlertOptions.RapidFlash, CoinInLevel3Color, ButtonLed, 1);
            _propertiesManager.ResetCalls();

            var log = new GameHistoryLog(0) {EndCredits = 0};
            _target.GameEnded(log);
            // Wait for CoinIn SessionTimeout(100ms) to expire and expect CoinIn state to go idle
            Thread.Sleep(125);
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            VerifyRateOfPlayIsPersistedAsZeroOnce();
            VerifyActiveCoinInRewardPersistedAsNull();
        }

        [TestMethod]
        public void PlayerCashoutResultsInZeroBalance()
        {
            // On zero balance, expect Coin-In session to reset after 50ms
            _target.CreditsWagered(1000000);
            Verify(BarkeeperAlertOptions.RapidFlash, CoinInLevel3Color, ButtonLed, 1);
            _bank.Setup(x => x.Balance).Returns(0);
            _propertiesManager.ResetCalls();

            _target.OnCashOutCompleted();
            Verify(BarkeeperAlertOptions.LightOn, CoinInIdleColor, ButtonLed, 1);
            VerifyRateOfPlayIsPersistedAsZeroOnce();
            VerifyActiveCoinInRewardPersistedAsNull();
        }

        [TestMethod]
        public void LastCoinInAwardPersistent()
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCoinIn, 0)).Returns(200000);
            _target.CreditsWagered(200000);
            Verify(BarkeeperAlertOptions.RapidFlash, CoinInLevel3Color, ButtonLed, 1);

            // Setup LastReward as CoinIn Level-3
            SetupPersistence(
                0,
                0,
                _target.RewardLevels.RewardLevels,
                _target.RewardLevels.CoinInStrategy,
                _target.RewardLevels.CashInStrategy,
                _target.RewardLevels.RewardLevels.Where(x => x.TriggerStrategy == BarkeeperStrategy.CoinIn)
                    .ToList()[2]);
            _edgeLightController.ResetCalls();
            _bank.Setup(x => x.Balance).Returns(200000);
            _target = new BarkeeperHandler(
                _edgeLightController.Object,
                _propertiesManager.Object,
                _gamePlayState.Object,
                _bank.Object);
            _target.Initialize();
            Verify(BarkeeperAlertOptions.RapidFlash, CoinInLevel3Color, ButtonLed, 1);
        }

        [TestMethod]
        public void CoinInLevelsWithRateOfPlayEnabled()
        {
            // Rate of play is 60ms for $1. So bet $1 and wait for 60ms to elapse and expect CoinIn reward state to be in Idle
            VerifyRewardLevel1();
            _propertiesManager.ResetCalls();
            Thread.Sleep(125);
            Assert.AreEqual(0, _target.CoinInDuringSession);
            VerifyRateOfPlayIsPersistedAsZeroOnce();
        }

        private void Verify(BarkeeperAlertOptions alert, Color color, List<int> lights, int count)
        {
            long flashTime;

            switch (alert)
            {
                case BarkeeperAlertOptions.LightOn:
                    VerifySolidColorPattern();
                    break;
                case BarkeeperAlertOptions.SlowFlash:
                    flashTime = SlowFlashTime;
                    VerifyBlinkPattern();
                    break;
                case BarkeeperAlertOptions.MediumFlash:
                    flashTime = MediumFlashTime;
                    VerifyBlinkPattern();
                    break;
                case BarkeeperAlertOptions.RapidFlash:
                    flashTime = FastFlashTime;
                    VerifyBlinkPattern();
                    break;
            }

            void VerifySolidColorPattern()
            {
                _edgeLightController.Verify(
                    x => x.AddEdgeLightRenderer(
                        It.Is<SolidColorPatternParameters>(
                            p => p.Priority == StripPriority.PlatformControlled && p.Color == color &&
                                 lights.SequenceEqual(p.Strips))),
                    Times.Exactly(count));
            }

            void VerifyBlinkPattern()
            {
                _edgeLightController.Verify(
                    x => x.AddEdgeLightRenderer(
                        It.Is<BlinkPatternParameters>(
                            p => p.Priority == StripPriority.PlatformControlled && p.OnColor == color &&
                                 p.OffColor == Color.Black && p.OnTime == flashTime && p.OffTime == flashTime)),
                    Times.Exactly(count));
            }
        }

        private void SetupPersistence(
            long cashInAmount,
            long coinInAmount,
            IReadOnlyCollection<RewardLevel> rewardLevels,
            CoinInStrategy coinInStrategy,
            CashInStrategy cashInStrategy,
            RewardLevel lastAward = null,
            bool enabled = true)
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCoinIn, 0)).Returns(10000);

            var barkeeperRewardLevels = new BarkeeperRewardLevels
            {
                RewardLevels = rewardLevels.ToArray(),
                CashInStrategy = cashInStrategy,
                CoinInStrategy = coinInStrategy,
                Enabled = enabled
            };

            var xml = _target.ToXml(barkeeperRewardLevels);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperRewardLevels, It.IsAny<string>())).Returns(xml);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperActiveCoinInReward, It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(lastAward));

            _target.RewardLevels = barkeeperRewardLevels;
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCoinIn, It.IsAny<long>())).Returns(coinInAmount);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.BarkeeperCashIn, It.IsAny<long>())).Returns(cashInAmount);
        }

        private CoinInStrategy GetCoinInStrategy()
        {
            return new CoinInStrategy { CoinInRate = new CoinInRate() };
        }


        /// <summary>
        /// Verify Reward Level 1 assuming this is the 1st method to run
        /// </summary>
        private void VerifyRewardLevel1()
        {
            // Bet $1 and expect Level-1 to hit
            _target.CreditsWagered(100000);
            Assert.AreEqual(100000, _target.CoinInDuringSession);
            Verify(BarkeeperAlertOptions.SlowFlash, CoinInLevel1Color, ButtonLed, 1);
            _edgeLightController.ResetCalls();
        }

        /// <summary>
        /// Verify Reward Level 2 assuming VerifyRewardLevel1 run once before
        /// <see cref="VerifyRewardLevel1"/>
        /// </summary>
        private void VerifyRewardLevel2(int count = 1)
        {
            // Bet another $0.50 and expect Level-2 to hit
            _target.CreditsWagered(50000);
            Assert.AreEqual(150000, _target.CoinInDuringSession);
            Verify(BarkeeperAlertOptions.MediumFlash, CoinInLevel2Color, ButtonLed, count);
            _edgeLightController.ResetCalls();
        }

        /// <summary>
        /// Verify Reward Level 3 assuming VerifyRewardLevel1 and VerifyRewardLevel2 run once before
        /// </summary>
        private void VerifyRewardLevel3(int count = 1)
        {
            // Bet another $0.50 and expect Level-2 to hit
            _target.CreditsWagered(50000);
            Assert.AreEqual(200000, _target.CoinInDuringSession);
            // Reward Level-3 is reached
            Verify(BarkeeperAlertOptions.RapidFlash, CoinInLevel3Color, ButtonLed, count);
            _edgeLightController.ResetCalls();
        }

        /// <summary>
        /// Verify the ROP elapsed is persisted as 0
        /// </summary>
        private void VerifyRateOfPlayIsPersistedAsZeroOnce()
        {
            _propertiesManager.Verify(
                mock => mock.SetProperty(GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds, 0L, true),
                Times.Once());
        }

        /// <summary>
        /// Verify we don't modify the Awarded flags are false in _target.RewardLevels
        /// </summary>
        private void VerifyAwardedFlagsAreFalse()
        {
            foreach (var rewardLevel in _target.RewardLevels.RewardLevels)
            {
                Assert.AreEqual(false, rewardLevel.Awarded);
            }
        }

        /// <summary>
        /// Verify persistence of ActiveCoinIn Reward level
        /// </summary>
        private void VerifyActiveCoinInRewardPersistence(RewardLevel originalRewardLevel, bool isAwarded = true)
        {
            RewardLevel rewardLevel = originalRewardLevel.DeepClone();
            rewardLevel.Awarded = isAwarded;
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.BarkeeperActiveCoinInReward,
                    JsonConvert.SerializeObject(rewardLevel),
                    true), Times.Once());
        }

        /// <summary>
        /// Verify persistence of ActiveCoinIn Reward level is set to NULL
        /// </summary>
        private void VerifyActiveCoinInRewardPersistedAsNull()
        {
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.BarkeeperActiveCoinInReward, JsonConvert.SerializeObject(null),
                    true), Times.Once());
        }
    }
}