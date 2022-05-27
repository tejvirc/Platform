namespace Aristocrat.Monaco.Bingo.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.AspNetCore.WebUtilities;
    using OverlayServer;
    using OverlayServer.Attributes;
    using Protocol.Common.Storage.Entity;

    public class LegacyAttractProvider : ILegacyAttractProvider
    {
        private const string GameNameParameter = "gameName";
        private const string PaytableNameParameter = "paytableName";
        private const string BaseUrlParameter = "baseUrl";
        private const string MajorUnitSeparatorParameter = "majorUnitSeparator";
        private const string MinorUnitSeparatorParameter = "minorUnitSeparator";
        private const string MinorDigitsParameter = "minorDigits";
        private const string BetAmountFormatParameter = "betFormat";
        private const string PayAmountFormatParameter = "payoutFormat";
        private const string BallsWithInAmountFormatParameter = "ballsInFormat";
        private const string UseCreditsParameter = "useCredits";
        private const string PatternCycleTimeParameter = "patternCycleMs";

        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public LegacyAttractProvider(
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _propertiesManager = propertiesManager;
            _gameProvider = gameProvider;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public Uri GetLegacyAttractUri(BingoDisplayConfigurationBingoAttractSettings attractSettings)
        {
            if (!attractSettings.CyclePatterns)
            {
                return null;
            }

            return new UriBuilder(QueryHelpers.AddQueryString(BingoConstants.BingoOverlayServerUri, GetUrlParameters(attractSettings)))
            {
                Path = OverlayType.Attract.GetOverlayRoute()
            }.Uri;
        }

        private IDictionary<string, string> GetUrlParameters(BingoDisplayConfigurationBingoAttractSettings attractSettings)
        {
            var gameId = _propertiesManager.GetValue(GamingConstants.SelectedGameId, 0);
            var game = _gameProvider.GetGame(gameId);
            var serverSettings = _unitOfWorkFactory.Invoke(
                x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault());

            var numberFormat = CurrencyExtensions.CurrencyCultureInfo.NumberFormat;
            var helpUri = new UriBuilder(_unitOfWorkFactory.GetHelpUri(_propertiesManager))
            {
                Path = string.Empty,
                Query = string.Empty
            }.Uri;

            return new Dictionary<string, string>
            {
                { GameNameParameter, serverSettings?.GameTitles ?? game.ThemeName },
                { PaytableNameParameter, serverSettings?.PaytableIds ?? game.PaytableId },
                { BaseUrlParameter, helpUri.ToString() },
                { MajorUnitSeparatorParameter, numberFormat.CurrencyGroupSeparator },
                { MinorUnitSeparatorParameter, numberFormat.CurrencyDecimalSeparator },
                { MinorDigitsParameter, numberFormat.NumberDecimalDigits.ToString() },
                { BallsWithInAmountFormatParameter, attractSettings.BallsCalledWithinFormattingText },
                { BetAmountFormatParameter, attractSettings.BetAmountFormattingText },
                { PayAmountFormatParameter, attractSettings.PayAmountFormattingText },
                { UseCreditsParameter, attractSettings.DisplayAmountsAsCredits.ToString() },
                { PatternCycleTimeParameter, attractSettings.PatternCycleTimeMilliseconds.ToString() }
            };
        }
    }
}