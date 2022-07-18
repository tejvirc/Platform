namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;

    /// <summary>
    ///     Handler for sending enabled features from games or the EGM broadly.
    /// </summary>
    public class LPA0SendEnabledFeaturesHandler : ISasLongPollHandler<LongPollSendEnabledFeaturesResponse,
        LongPollSingleValueData<uint>>
    {
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates a new instance of the LPA0SendEnabledFeaturesHandler class.
        /// </summary>
        public LPA0SendEnabledFeaturesHandler(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll>() { LongPoll.SendEnabledFeatures };

        /// <inheritdoc />
        /// <param name="data">
        ///     Single value uint used to identify a target game. SAS requires it as an argument,
        ///     but it is currently not in use.
        /// </param>
        public LongPollSendEnabledFeaturesResponse Handle(LongPollSingleValueData<uint> data)
        {
            var portAssignment = _propertiesManager.GetValue(SasProperties.SasPortAssignments, new PortAssignment());
            var settings = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            return new LongPollSendEnabledFeaturesResponse
            {
                Features1Data = GetFeatures1(portAssignment, settings),
                Features2Data = GetFeatures2(portAssignment, settings),
                Features3Data = GetFeatures3(portAssignment),
                Features4Data = GetFeatures4(portAssignment)
            };
        }
        
        private LongPollSendEnabledFeaturesResponse.Features1 GetFeatures1(PortAssignment portAssignment, SasFeatures features)
        {
            var featuresGotten = (LongPollSendEnabledFeaturesResponse.Features1)0;
            if (_propertiesManager.GetValue(SasProperties.JackpotMultiplierSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.JackpotMultiplier;
            }

            if (portAssignment.FundTransferPort != HostId.None && features.AftBonusAllowed)
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.AftBonusAwards;
            }

            if (portAssignment.LegacyBonusPort != HostId.None && features.LegacyBonusAllowed)
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.LegacyBonusAwards;
            }

            if (_propertiesManager.GetValue(SasProperties.TournamentSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.Tournament;
            }

            switch (_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType)
            {
                case SasValidationType.System:
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions;
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit0;
                    break;
                case SasValidationType.SecureEnhanced:
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.ValidationExtensions;
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.ValidationStyleBit1;
                    break;
            }

            if (_propertiesManager.GetValue(SasProperties.TicketRedemptionSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features1.TicketRedemption;
            }

            return featuresGotten;
        }
        
        private LongPollSendEnabledFeaturesResponse.Features2 GetFeatures2(PortAssignment portAssignment, SasFeatures features)
        {
            var featuresGotten = (LongPollSendEnabledFeaturesResponse.Features2)0;
            switch (_propertiesManager.GetValue(SasProperties.MeterModelKey, SasMeterModel.NotSpecified))
            {
                case SasMeterModel.MeteredWhenWon:
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit0;
                    break;
                case SasMeterModel.MeteredWhenPlayed:
                    featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.MeterModelBit1;
                    break;
            }

            if (_propertiesManager.GetValue(SasProperties.TicketsToDropMetersKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2
                    .TicketsToTotalDropAndTotalCancelledCredits;
            }

            if (_propertiesManager.GetValue(SasProperties.ExtendedMetersSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.ExtendedMeters;
            }

            if (_propertiesManager.GetValue(SasProperties.ComponentAuthenticationSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.ComponentAuthentication;
            }

            if (_propertiesManager.GetValue(SasProperties.JackpotKeyoffExceptionSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.JackpotKeyoffToMachinePayException;
            }

            if (portAssignment.FundTransferPort != HostId.None && features.AftAllowed)
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.AdvancedFundTransfer;
            }

            if (_propertiesManager.GetValue(SasProperties.MultiDenomExtensionsSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features2.MultidenomExtensions;
            }

            return featuresGotten;
        }

        private LongPollSendEnabledFeaturesResponse.Features3 GetFeatures3(PortAssignment portAssignment)
        {
            var featuresGotten = (LongPollSendEnabledFeaturesResponse.Features3)0;
            if (_propertiesManager.GetValue(SasProperties.MaxPollingRateSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.MaxPollingRateBit0;
            }

            if (portAssignment.ProgressivePort != HostId.None &&
                _propertiesManager.GetValue(SasProperties.MultipleSasProgressiveWinReportingSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.MultipleSasProgressiveWinReporting;
            }

            if (_propertiesManager.GetValue(SasProperties.MeterChangeNotificationSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.MeterChangeNotification;
            }

            if (_propertiesManager.GetValue(SasProperties.SessionPlaySupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.SessionPlay;
            }

            if (_propertiesManager.GetValue(SasProperties.ForeignCurrencyRedemptionSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.ForeignCurrencyRedemption;
            }

            if (_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).NonSasProgressiveHitReporting)
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.NonSasProgressiveHitReporting;
            }

            if (portAssignment.ProgressivePort != HostId.None &&
                _propertiesManager.GetValue(SasProperties.EnhancedProgressiveDataReportingKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features3.EnhancedProgressiveDataReporting;
            }

            return featuresGotten;
        }
        
        private LongPollSendEnabledFeaturesResponse.Features4 GetFeatures4(PortAssignment portAssignment)
        {
            var featuresGotten = (LongPollSendEnabledFeaturesResponse.Features4)0;

            if (portAssignment.ProgressivePort != HostId.None &&
                _propertiesManager.GetValue(SasProperties.MaxProgressivePaybackSupportedKey, false))
            {
                featuresGotten |= LongPollSendEnabledFeaturesResponse.Features4.MaxProgressivePayback;
            }

            return featuresGotten;
        }
    }
}