namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LPA0SendEnabledFeaturesHandlerTest
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private SasFeatures _defaultFeatures = new SasFeatures
        {
            AftBonusAllowed = true,
            LegacyBonusAllowed = false,
            TransferInAllowed = true,
            TransferOutAllowed = true,
            ValidationType = SasValidationType.SecureEnhanced
        };

        private PortAssignment _defaultPortAssignment = new PortAssignment
        {
            FundTransferPort = HostId.Host1,
            LegacyBonusPort = HostId.Host1,
            ProgressivePort = HostId.Host1,
            ValidationPort = HostId.Host1,
            GameStartEndHosts = GameStartEndHost.Both,
            IsDualHost = true,
            GeneralControlPort = HostId.Host1
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            SetupProperties();
        }

        [TestMethod]
        public void CommandsTest()
        {
            var target = new LPA0SendEnabledFeaturesHandler(_propertiesManager.Object);
            Assert.AreEqual(1, target.Commands.Count);
            Assert.IsTrue(target.Commands.Contains(LongPoll.SendEnabledFeatures));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _ = new LPA0SendEnabledFeaturesHandler(null);
        }

        [TestMethod]
        public void NoGameSpecifiedTest()
        {
            var target = new LPA0SendEnabledFeaturesHandler(_propertiesManager.Object);

            var expectedFeatures1Data = LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier |
                LongPollSendEnabledFeaturesResponse.Features1.AftBonusAwards |
                LongPollSendEnabledFeaturesResponse.Features1.Tournament |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1;
            var expectedFeatures2Data = LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit0 |
                LongPollSendEnabledFeaturesResponse.Features2.ExtendedMeters |
                LongPollSendEnabledFeaturesResponse.Features2.ComponentAuthentication |
                LongPollSendEnabledFeaturesResponse.Features2.JackpotKeyoffToMachinePayException |
                LongPollSendEnabledFeaturesResponse.Features2.AdvancedFundTransfer |
                LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            var expectedFeatures3Data = LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification |
                LongPollSendEnabledFeaturesResponse.Features3.SessionPlay;
            var expectedFeatures4Data = LongPollSendEnabledFeaturesResponse.Features4.MaxProgressivePayback;

            var result = target.Handle(new LongPollSingleValueData<uint>(0));

            Assert.AreEqual(expectedFeatures1Data, result.Features1Data);
            Assert.AreEqual(expectedFeatures2Data, result.Features2Data);
            Assert.AreEqual(expectedFeatures3Data, result.Features3Data);
            Assert.AreEqual(expectedFeatures4Data, result.Features4Data);
        }

        [TestMethod]
        public void NoGameSpecifiedAllFeatures1Test()
        {
            var target = new LPA0SendEnabledFeaturesHandler(_propertiesManager.Object);
            _defaultFeatures.LegacyBonusAllowed = true;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(_defaultFeatures);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.TicketRedemptionSupportedKey), It.IsAny<object>()))
                .Returns(true);

            var expectedFeatures1Data = LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier |
                LongPollSendEnabledFeaturesResponse.Features1.AftBonusAwards |
                LongPollSendEnabledFeaturesResponse.Features1.LegacyBonusAwards |
                LongPollSendEnabledFeaturesResponse.Features1.Tournament |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1 |
                LongPollSendEnabledFeaturesResponse.Features1.TicketRedemption;
            var expectedFeatures2Data = LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit0 |
                LongPollSendEnabledFeaturesResponse.Features2.ExtendedMeters |
                LongPollSendEnabledFeaturesResponse.Features2.ComponentAuthentication |
                LongPollSendEnabledFeaturesResponse.Features2.JackpotKeyoffToMachinePayException |
                LongPollSendEnabledFeaturesResponse.Features2.AdvancedFundTransfer |
                LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            var expectedFeatures3Data = LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification |
                LongPollSendEnabledFeaturesResponse.Features3.SessionPlay;
            var expectedFeatures4Data = LongPollSendEnabledFeaturesResponse.Features4.MaxProgressivePayback;

            var result = target.Handle(new LongPollSingleValueData<uint>(0));

            Assert.AreEqual(expectedFeatures1Data, result.Features1Data);
            Assert.AreEqual(expectedFeatures2Data, result.Features2Data);
            Assert.AreEqual(expectedFeatures3Data, result.Features3Data);
            Assert.AreEqual(expectedFeatures4Data, result.Features4Data);
        }


        [TestMethod]
        public void NoGameSpecifiedAllFeatures1NoComPortsTest()
        {
            var target = new LPA0SendEnabledFeaturesHandler(_propertiesManager.Object);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.TicketRedemptionSupportedKey), It.IsAny<object>())).Returns(true);
            _defaultPortAssignment.FundTransferPort = HostId.None;
            _defaultPortAssignment.LegacyBonusPort = HostId.None;
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasPortAssignments, It.IsAny<object>())).Returns(_defaultPortAssignment);

            var expectedFeatures1Data = LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier |
                LongPollSendEnabledFeaturesResponse.Features1.Tournament |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1 |
                LongPollSendEnabledFeaturesResponse.Features1.TicketRedemption;
            var expectedFeatures2Data = LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit0 |
                LongPollSendEnabledFeaturesResponse.Features2.ExtendedMeters |
                LongPollSendEnabledFeaturesResponse.Features2.ComponentAuthentication |
                LongPollSendEnabledFeaturesResponse.Features2.JackpotKeyoffToMachinePayException |
                LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            var expectedFeatures3Data = LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification |
                LongPollSendEnabledFeaturesResponse.Features3.SessionPlay;
            var expectedFeatures4Data = LongPollSendEnabledFeaturesResponse.Features4.MaxProgressivePayback;

            var result = target.Handle(new LongPollSingleValueData<uint>(0));

            Assert.AreEqual(expectedFeatures1Data, result.Features1Data);
            Assert.AreEqual(expectedFeatures2Data, result.Features2Data);
            Assert.AreEqual(expectedFeatures3Data, result.Features3Data);
            Assert.AreEqual(expectedFeatures4Data, result.Features4Data);
        }

        [TestMethod]
        public void GameSpecifiedTest()
        {
            var target = new LPA0SendEnabledFeaturesHandler(_propertiesManager.Object);

            var expectedFeatures1Data = LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier |
                LongPollSendEnabledFeaturesResponse.Features1.AftBonusAwards |
                LongPollSendEnabledFeaturesResponse.Features1.Tournament |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions |
                LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1;
            var expectedFeatures2Data = LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit0 |
                LongPollSendEnabledFeaturesResponse.Features2.ExtendedMeters |
                LongPollSendEnabledFeaturesResponse.Features2.ComponentAuthentication |
                LongPollSendEnabledFeaturesResponse.Features2.JackpotKeyoffToMachinePayException |
                LongPollSendEnabledFeaturesResponse.Features2.AdvancedFundTransfer |
                LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            var expectedFeatures3Data = LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification |
                LongPollSendEnabledFeaturesResponse.Features3.SessionPlay;
            var expectedFeatures4Data = LongPollSendEnabledFeaturesResponse.Features4.MaxProgressivePayback;

            var result = target.Handle(new LongPollSingleValueData<uint>(1));

            Assert.AreEqual(expectedFeatures1Data, result.Features1Data);
            Assert.AreEqual(expectedFeatures2Data, result.Features2Data);
            Assert.AreEqual(expectedFeatures3Data, result.Features3Data);
            Assert.AreEqual(expectedFeatures4Data, result.Features4Data);
        }

        private void SetupProperties()
        {
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.JackpotMultiplierSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.TournamentSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.TicketRedemptionSupportedKey), It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(_defaultFeatures);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasPortAssignments, It.IsAny<object>())).Returns(_defaultPortAssignment);

            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MeterModelKey), It.IsAny<object>())).Returns(SasMeterModel.MeteredWhenWon);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.TicketsToDropMetersKey), It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.ExtendedMetersSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.ComponentAuthenticationSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.JackpotKeyoffExceptionSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MultiDenomExtensionsSupportedKey), It.IsAny<object>())).Returns(true);

            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MaxPollingRateSupportedKey), It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MultipleSasProgressiveWinReportingSupportedKey), It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MeterChangeNotificationSupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.SessionPlaySupportedKey), It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.ForeignCurrencyRedemptionSupportedKey), It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.EnhancedProgressiveDataReportingKey), It.IsAny<object>())).Returns(false);

            _propertiesManager.Setup(c => c.GetProperty(It.Is<string>(d => d == SasProperties.MaxProgressivePaybackSupportedKey), It.IsAny<object>())).Returns(true);
        }
    }
}
