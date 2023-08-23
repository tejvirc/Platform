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
    using OverlayServer.Data.Bingo;
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
        private const string PatternsUrlFormatParameter = "patternsUrlFormat";
        private const string DefaultAttractPatternJsFile = "scripts/pattern.js";
        private const string HtmlPageEnding = ".html";

        private readonly IGameProvider _gameProvider;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public LegacyAttractProvider(
            IGameProvider gameProvider,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
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
            var (game, denomination) = _gameProvider.GetActiveGame();
            var serverSettings = _unitOfWorkFactory.Invoke(
                    x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault())?.GamesConfigured
                ?.FirstOrDefault(c => c.PlatformGameId == game.Id && c.Denomination == denomination.Value);

            var numberFormat = CurrencyExtensions.CurrencyCultureInfo.NumberFormat;
            var helpUrl = _unitOfWorkFactory.GetHelpUri(_gameProvider);
            var helpUriBuilder = new UriBuilder(helpUrl);

            if (helpUriBuilder.Path.EndsWith(HtmlPageEnding))
            {
                var htmlPage = helpUriBuilder.Path.Split('/').Last();
                helpUriBuilder.Path = helpUriBuilder.Path.Replace(htmlPage, string.Empty);
            }

            var helpPath = helpUriBuilder.Path.TrimEnd('/');
            helpUriBuilder.Path = string.Empty;
            helpUriBuilder.Query = string.Empty;

            return new Dictionary<string, string>
            {
                { GameNameParameter, serverSettings?.GameTitleId.ToString() ?? game.ThemeName },
                { PaytableNameParameter, serverSettings?.PaytableId.ToString() ?? game.PaytableId },
                { BaseUrlParameter, helpUriBuilder.Uri.ToString() },
                { MajorUnitSeparatorParameter, numberFormat.CurrencyGroupSeparator },
                { MinorUnitSeparatorParameter, numberFormat.CurrencyDecimalSeparator },
                { MinorDigitsParameter, numberFormat.NumberDecimalDigits.ToString() },
                { BallsWithInAmountFormatParameter, attractSettings.BallsCalledWithinFormattingText },
                { BetAmountFormatParameter, attractSettings.BetAmountFormattingText },
                { PayAmountFormatParameter, attractSettings.PayAmountFormattingText },
                { UseCreditsParameter, attractSettings.DisplayAmountsAsCredits.ToString() },
                { PatternCycleTimeParameter, attractSettings.PatternCycleTimeMilliseconds.ToString() },
                { PatternsUrlFormatParameter, $"{helpPath}/{DefaultAttractPatternJsFile}" }
            };
        }
    }
}