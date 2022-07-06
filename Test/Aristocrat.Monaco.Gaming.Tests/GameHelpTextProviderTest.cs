namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Currency;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Progressives;
    using Gaming.Runtime;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameHelpTextProviderTest
    {
        private Mock<IPropertiesManager> _properties;
        private Mock<IEventBus> _eventBus;
        private Mock<IRuntime> _runtime;
        private Mock<IProgressiveGameProvider> _progressive;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, CultureInfo.CurrentCulture, "c");
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, CultureInfo.CurrentCulture);

            _runtime = new Mock<IRuntime>();
            _properties = new Mock<IPropertiesManager>(MockBehavior.Loose);
            _eventBus = new Mock<IEventBus>();
            _progressive = new Mock<IProgressiveGameProvider>();

            _properties.Setup(mock => mock.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>())).Returns(false);
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinRatio, It.IsAny<long>())).Returns(0L);
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>())).Returns(0L);
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinRatioThreshold, It.IsAny<long>())).Returns(0L);
            _properties.Setup(mock => mock.GetProperty(GamingConstants.DisplayProgressiveCeilingMessage, It.IsAny<bool>())).Returns(false);
            _properties.Setup(mock => mock.GetProperty(GamingConstants.ReelStopEnabled, It.IsAny<bool>())).Returns(false);
            _properties.Setup(mock => mock.GetProperty(GamingConstants.DisplayStopReelMessage, It.IsAny<bool>())).Returns(false);
        }

        [DataRow(true, false, false, false, DisplayName = "Null Runtime")]
        [DataRow(false, true, false, false, DisplayName = "Null PropertiesManager")]
        [DataRow(false, false, true, false, DisplayName = "Null EventBus")]
        [DataRow(false, false, false, true, DisplayName = "Null ProgressiveGameProvider")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(bool nullRuntime, bool nullPropertiesManager,
            bool nullEventBus, bool nullProgressiveGameProvider)
        {
            _ = ConstructGameHelpTextService(nullRuntime, nullPropertiesManager,
                nullEventBus, nullProgressiveGameProvider);
        }

        private GameHelpTextProvider ConstructGameHelpTextService(bool nullRuntime, bool nullPropertiesManager, bool nullEventBus, bool nullProgressiveGameProvider)
        {
            return new GameHelpTextProvider(nullPropertiesManager ? null : _properties.Object,
                 nullEventBus ? null : _eventBus.Object,
                nullRuntime ? null : _runtime.Object,
                nullProgressiveGameProvider ? null : _progressive.Object);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void ServiceInitialization()
        {
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            service.Initialize();

            _eventBus.Verify(
                b => b.Subscribe(
                    It.Is<object>(o => o == service),
                    It.IsAny<Action<PropertyChangedEvent>>(),
                    It.IsAny<Predicate<PropertyChangedEvent>>()));
        }

        [TestMethod]
        public void HelpTextCount()
        {
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            var helpTexts = service.AllHelpTexts;

            Assert.AreEqual(1, helpTexts.Count);
        }

        [DataRow(DisplayName = "HelpTextWhenVoucherOutDisabledAndNoLargeWinLimit")]
        [DataRow(DisplayName = "HelpTextWhenDoubleTapIsDisabled")]
        [DataTestMethod]
        public void HelpTextWhenEverythingIsDisabled()
        {
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            Assert.AreEqual(string.Empty, service.AllHelpTexts.Values.ToArray()[0]());
        }

        [TestMethod]
        public void HelpTextWithoutPrinter()
        {
            // no printer but has VoucherOut
            MoqServiceManager.RemoveService<IPrinter>();
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>())).Returns(true);

            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            Assert.AreEqual(string.Empty, service.AllHelpTexts.Values.ToArray()[0]());
        }

        [TestMethod]
        public void HelpTextWithoutLargeWinLimit()
        {
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>())).Returns(true);
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            var expected = "All pays will be paid by a printed voucher.\r\n";
            var actual = service.AllHelpTexts.Values.ToArray()[0]();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HelpTextWithVoucherOutDisabled()
        {
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>())).Returns(10L);

            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            var actual = service.AllHelpTexts.Values.ToArray()[0]();
            Assert.IsTrue(
                !actual.Contains("All pays will be paid by a printed voucher")
                && actual.Contains("will be paid by the attendant"));
        }

        [TestMethod]
        public void HelpTextWithLargeWinRatio()
        {
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>())).Returns(10L);
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinRatioThreshold, It.IsAny<long>())).Returns(123000L);
            _properties.Setup(mock => mock.GetProperty(AccountingConstants.LargeWinRatio, It.IsAny<long>())).Returns(456L);

            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            var actual = service.AllHelpTexts.Values.ToArray()[0]();
            Assert.IsTrue(
                !actual.Contains("All pays will be paid by a printed voucher")
                && actual.Contains("original wager amount) greater than or equal to $1.23 and 4.56x wager"));
        }

         [TestMethod]
        public void HelpTextWithProgressiveLevels()
        {
            var TestProgressiveLevels = new List<IViewableProgressiveLevel>();
            TestProgressiveLevels.Add(new ProgressiveLevel{LevelName = "MajorLevel" , MaximumValue = 1000000000L });
            TestProgressiveLevels.Add(new ProgressiveLevel{LevelName = "MediumLevel" , MaximumValue = 800000000L});
            TestProgressiveLevels.Add(new ProgressiveLevel{LevelName = "MinorLevel" , MaximumValue = 500000000L });
            _properties.Setup(mock => mock.GetProperty(GamingConstants.DisplayProgressiveCeilingMessage, It.IsAny<bool>())).Returns(true);

            var _progressiveCustom = MoqServiceManager.CreateAndAddService<IProgressiveGameProvider>(MockBehavior.Strict);
            _progressiveCustom.Setup(mock => mock.GetActiveProgressiveLevels()).Returns(TestProgressiveLevels);
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressiveCustom.Object);
            var helpText = service.AllHelpTexts.Values.ToArray()[0]();
            Assert.IsTrue(helpText.Contains("Maximum MajorLevel progressive meter amount is $10,000.00")&&
                helpText.Contains("Maximum MediumLevel progressive meter amount is $8,000.00") &&
                helpText.Contains("Maximum MinorLevel progressive meter amount is $5,000.00"));
        }

        [TestMethod]
        public void HelpTextWithDoubleTap()
        {
            _properties.Setup(mock => mock.GetProperty(GamingConstants.ReelStopEnabled, It.IsAny<bool>())).Returns(true);
            _properties.Setup(mock => mock.GetProperty(GamingConstants.DisplayStopReelMessage, It.IsAny<bool>())).Returns(true);

            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressive.Object);
            var actual = service.AllHelpTexts.Values.ToArray()[0]();
            Assert.IsTrue(actual.Contains("While the reels are spinning you may press the big button to stop them,") &&
                actual.Contains("except during some special features/bonuses where applicable."));
        }

        [TestMethod]
        public void HelpTextWhenProgressiveLevelsAreDisabled()
        {
            var TestProgressiveLevels = new List<IViewableProgressiveLevel>();
            TestProgressiveLevels.Add(new ProgressiveLevel { LevelName = "MajorLevel", MaximumValue = 1000000000L });
            TestProgressiveLevels.Add(new ProgressiveLevel { LevelName = "MediumLevel", MaximumValue = 800000000L });
            TestProgressiveLevels.Add(new ProgressiveLevel { LevelName = "MinorLevel", MaximumValue = 500000000L });

            var _progressiveCustom = MoqServiceManager.CreateAndAddService<IProgressiveGameProvider>(MockBehavior.Strict);
            _progressiveCustom.Setup(mock => mock.GetActiveProgressiveLevels()).Returns(TestProgressiveLevels);
            var service = new GameHelpTextProvider(
                _properties.Object,
                _eventBus.Object,
                _runtime.Object,
                _progressiveCustom.Object);

            Assert.AreEqual(string.Empty, service.AllHelpTexts.Values.ToArray()[0]());
        }
    }

}