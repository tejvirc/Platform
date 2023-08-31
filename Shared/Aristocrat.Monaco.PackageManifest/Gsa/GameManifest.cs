namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;
    using Models;

    /// <summary>
    ///     Implementation of the GSA game manifest
    /// </summary>
    public class GameManifest : GsaManifest, IManifest<GameContent>
    {
        private const string GameType = @"G2S_game";

        private const string DefaultVariationId = "99";

        private const int DefaultUniqueGameId = 0;

        /// <inheritdoc />
        public GameContent Read(string file)
        {
            var manifest = Parse(file);

            // Throws on error
            Validate(manifest);

            var product = manifest.productList.product.FirstOrDefault();
            if (product == null)
            {
                throw new InvalidManifestException("The manifest does not contain a product.");
            }

            // Get the localized text if present, otherwise get the first one
            var localizedInfo = GetLocalization(product);

            var game = new GameContent
            {
                Name = localizedInfo.productName,
                ProductId = product.productId,
                GameDll = product.productGameDll,
                IconType = product.iconType,
                ManifestId = manifest.manifestId,
                ReleaseNumber = product.releaseNum,
                ReleaseDate = product.releaseDateTime,
                Description = localizedInfo.shortDesc,
                DetailedDescription = localizedInfo.longDesc,
                InstallSequence = "<installSeq />",
                UninstallSequence = "<uninstallSeq />",
                Package = Map(manifest.packageList, product.requiredPackage.FirstOrDefault()),
                GameAttributes = product.gameAttributes.Select(Map)
                    .OrderByDescending(g => g.VariationId == DefaultVariationId)
                    .ThenBy(g => Convert.ToInt32(g.VariationId)).ToList(),
                UpgradeActions = manifest.upgradeActionList?.upgradeAction?.Select(Map).ToList(),
                Configurations = product.configurationList?.configuration?.Select(Map).ToList(),
                DefaultConfiguration = Map(product.configurationList?.configuration?.FirstOrDefault(c => c.name.Equals(product.configurationList?.@default))),
                MechanicalReels = product.mechanicalReels,
                MechanicalReelHomeSteps = GetMechanicalReelHomeSteps(product),
                PreloadedAnimationFiles = product.stepperAnimationFileList
            };

            foreach (var l in product.localization)
            {
                game.Graphics.Add(l.localeCode, l.graphicElement.Select(Map).ToList());
            }

            return game;
        }

        /// <inheritdoc />
        public GameContent Read(Func<Stream> streamProvider)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void Validate(c_manifest manifest)
        {
            base.Validate(manifest);

            var product = manifest.productList.product.FirstOrDefault();
            if (product == null)
            {
                throw new InvalidManifestException("The manifest does not contain a product.");
            }

            if (product.productType != GameType)
            {
                throw new InvalidManifestException("The manifest is not a game manifest.");
            }

            if (product.gameAttributes == null)
            {
                throw new InvalidManifestException("The manifest does not contain any game attributes.");
            }
        }

        private static GameAttributes Map(c_gameAttributes gameInfo)
        {
            var betOptionList = new BetOptionList(gameInfo.Item?.betOption, gameInfo.betLinePresetList);
            var activeBetOptionName = gameInfo.Item?.activeOption;
            var activeBetOption = !string.IsNullOrEmpty(activeBetOptionName)
                ? betOptionList.FirstOrDefault(o => o.Name == activeBetOptionName)
                : null;

            var lineOptionList = new LineOptionList(gameInfo.lineOptionList?.lineOption);
            var activeLineOptionName = gameInfo.lineOptionList?.activeOption;
            var activeLineOption = !string.IsNullOrEmpty(activeLineOptionName)
                ? lineOptionList.FirstOrDefault(o => o.Name == activeLineOptionName)
                : null;

            var betLinePresetList = new BetLinePresetList(gameInfo.betLinePresetList, betOptionList, lineOptionList);

            // Verify that ActiveLineOption and ActiveBetOption pair are legal. If either of these
            // active options are null, this check can be skipped. This may be the case, for
            // example, if no BetLinePresets exist and no LineOptions exist.
            if (activeBetOption != null && activeLineOption != null)
            {
                if (!betLinePresetList.Any(preset =>
                    preset.BetOption.Equals(activeBetOption)
                    && preset.LineOption.Equals(activeLineOption)))
                {
                    throw new Exception("The default BetOption and default LineOption are an illegal pair, according to betLinePresetList");
                }
            }

            return new GameAttributes
            {
                ThemeId = gameInfo.themeId,
                PaytableId = gameInfo.paytableId,
                MaxPaybackPercent = GsaRtpHelper.NormalizeRtp(gameInfo.maxPaybackPct),
                MinPaybackPercent = GsaRtpHelper.NormalizeRtp(gameInfo.minPaybackPct),
                DisplayMeterName = gameInfo.displayMeterName,
                AssociatedSapDisplayMeterName = gameInfo.associatedSapDisplayMeterName,
                InitialValue = gameInfo.initialValue,
                SecondaryAllowed = gameInfo.secondaryAllowed,
                SecondaryEnabled = gameInfo.secondaryEnabled,
                LetItRideAllowed = gameInfo.letItRideAllowed,
                LetItRideEnabled = gameInfo.letItRideEnabled,
                Denominations = gameInfo.gameDenomList.Select(Map).ToList(),
                WagerCategories = gameInfo.wagerCategoryList.Select(Map).ToList(),
                CdsThemeId = gameInfo.cdsInfoList?.themeId,
                CdsTitleId = gameInfo.cdsInfoList?.titleId,
                CentralInfo = gameInfo.cdsInfoList?.cdsInfo.Select(Map).ToList() ?? Enumerable.Empty<CentralInfo>(),
                VariationId = gameInfo.variationId ?? GetVariationFromPaytableId(gameInfo.paytableId),
                GameType = gameInfo.gameTypeSpecified ? gameInfo.gameType : t_gameType.Slot,
                GameSubtype = gameInfo.gameSubtype,
                BetOptionList = betOptionList,
                ActiveBetOption = activeBetOption,
                LineOptionList = lineOptionList,
                ActiveLineOption = activeLineOption,
                BetLinePresetList = betLinePresetList,
                WinThreshold = gameInfo.winThresholdSpecified ? gameInfo.winThreshold : null,
                MaximumProgressivePerDenom = gameInfo.maxProgPerDenomSpecified ? gameInfo.maxProgPerDenom : null,
                ReferenceId = gameInfo.referenceId ?? string.Empty,
                Category = gameInfo.categorySpecified ? gameInfo.category : (t_category?)null,
                SubCategory = gameInfo.subCategorySpecified ? gameInfo.subCategory : (t_subCategory?)null,
                //BonusGames = gameInfo.bonusGameList
                SubGames = GetSubGames(gameInfo.subGameList),
                Features = gameInfo.FeatureList?.Where(feature => feature.StatInfo != null)
                    .Select(feature => new Feature
                    {
                        FeatureName = feature.FeatureName,
                        Name = feature.Name,
                        Editable = feature.Editable,
                        Enable = feature.Enabled,
                        StatInfo = feature.StatInfo?.Select(statInfo => new StatInfo { Name = statInfo.Name, DisplayName = statInfo.DisplayName }).ToList()
                    }).ToList(),
                MaxWagerInsideCredits = gameInfo.maxWagerInsideCredits,
                MaxWagerOutsideCredits = gameInfo.maxWagerOutsideCredits,
                NextToMaxBetTopAwardMultiplier = gameInfo.nextToMaxBetTopAwardMultiplier,
                UniqueGameId = gameInfo.uniqueGameIdSpecified ? gameInfo.uniqueGameId : DefaultUniqueGameId
            };
        }

        private static Graphic Map(graphicElement graphicElement)
        {
            GraphicType gfxType;
            switch (graphicElement.graphicType.ToUpper(CultureInfo.InvariantCulture))
            {
                case "PKG_ICON":
                    gfxType = GraphicType.Icon;
                    break;
                case "PKG_OTHER":
                    gfxType = GraphicType.Other;
                    break;
                case "PKG_ATTRACTVIDEO":
                    gfxType = GraphicType.AttractVideo;
                    break;
                case "PKG_LOADINGSCREEN":
                    gfxType = GraphicType.LoadingScreen;
                    break;
                case "PKG_BACKGROUNDPREVIEW":
                    gfxType = GraphicType.BackgroundPreview;
                    break;
                case "PKG_DENOMBUTTON":
                    gfxType = GraphicType.DenomButton;
                    break;
                case "PKG_DENOMPANEL":
                    gfxType = GraphicType.DenomPanel;
                    break;
                default:
                    gfxType = GraphicType.Icon;
                    break;
            }

            // Add the icon if it exists in the manifest
            return new Graphic
            {
                GraphicType = gfxType,
                Encoding = MapEncodingType(graphicElement.imageEncoding),
                FileName = graphicElement.fileName,
                Tags = graphicElement.tags?.ToLower().Split(',').ToHashSet()
            };
        }

        private static long Map(c_gameDenom denom)
        {
            return denom.denomId;
        }

        private static ImageEncodingType MapEncodingType(string value)
        {
            return (from ImageEncodingType type in Enum.GetValues(typeof(ImageEncodingType))
                    let converted = type.ToString()
                    where
                        converted.Equals(value.Replace(@"PKG_", string.Empty), StringComparison.InvariantCultureIgnoreCase)
                    select type).FirstOrDefault();
        }

        private static WagerCategory Map(c_wagerCategoryItem wagerCategory)
        {
            return new WagerCategory
            {
                Id = wagerCategory.wagerCategory.ToString(),
                MaxWagerCredits = wagerCategory.maxWagerCredits,
                MinWagerCredits = wagerCategory.minWagerCredits,
                MaxWinAmount = wagerCategory.maxWinAmount,
                TheoPaybackPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.theoPaybackPct),
                MinBaseRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.minBaseRtpPct),
                MaxBaseRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.maxBaseRtpPct),
                MinSapStartupRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.minSapStartupRtpPct),
                MaxSapStartupRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.maxSapStartupRtpPct),
                SapIncrementRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.sapIncrementRtpPct),
                MinLinkStartupRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.minLinkStartupRtpPct),
                MaxLinkStartupRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.maxLinkStartupRtpPct),
                LinkIncrementRtpPercent = GsaRtpHelper.NormalizeRtp(wagerCategory.linkIncrementRtpPct)
            };
        }

        private static UpgradeAction Map(c_upgradeAction upgradeAction)
        {
            return new UpgradeAction
            {
                FromPaytableId = upgradeAction.fromPaytableId,
                ToPaytableId = upgradeAction.toPaytableId,
                FromVersion = upgradeAction.fromVersion,
                DenomId = upgradeAction.denomId,
                MaintainPosition = upgradeAction.maintainPosition,
                MigrateJackpots = upgradeAction.migrateJackpots
            };
        }

        private static ManifestPackage Map(packageList packages, c_requiredPackage requiredPackage)
        {
            if (requiredPackage == null)
            {
                return null;
            }

            var package = packages?.package.FirstOrDefault(p => p.packageId == requiredPackage.packageId);
            if (package == null)
            {
                return null;
            }

            return new ManifestPackage
            {
                PackageId = package.packageId,
                ModuleId = package.moduleId,
                FileName = package.fileName,
                ReleaseNumber = package.releaseNum,
                PackageSize = package.packageSize,
                Dependency = Map(package.packageDependency)
            };
        }

        private static PackageDependency Map(c_packgeDependency dependency)
        {
            return new PackageDependency
            {
                Operator = dependency.@operator == "PKG_and" ? Operator.And : Operator.Or,
                Not = dependency.not,
                Dependencies = dependency.Items.Select(Map)
            };
        }

        private static Dependency Map(object item)
        {
            switch (item)
            {
                case c_moduleDependency dependency:
                    return new ModuleDependency
                    {
                        Pattern = dependency.moduleIdPattern,
                        Not = dependency.not
                    };
                case c_storageDependency dependency:
                    return new StorageDependency
                    {
                        Type = dependency.storageType,
                        Application = dependency.storageApplication,
                        Size = dependency.storageQty
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        private static CentralInfo Map(c_cdsInfo cdsInfo)
        {
            return new CentralInfo
            {
                Id = cdsInfo.id,
                Name = cdsInfo.name,
                Denomination = cdsInfo.denom,
                Bet = cdsInfo.bet,
                BetLinePresetId = cdsInfo.betLinePresetId,
                BetMultiplier = cdsInfo.betMultiplier,
                Upc = cdsInfo.upc
            };
        }

        private static Configuration Map(c_configuration configuration)
        {
            if (configuration == null)
            {
                return null;
            }

            return new Configuration
            {
                Name = configuration.name,
                // Using FirstOrDefault() because technically there is 1 gameConfiguration per Game Theme.
                // As of now (3/11/2021), the Platform does not support more than 1 game Theme per GSA Manifest,
                // so we are assuming the GSA Manifest does not have additional gameConfigurations
                GameConfiguration = Map(configuration.gameConfiguration.FirstOrDefault())
            };
        }

        private static GameConfiguration Map(c_gameConfiguration gameConfiguration)
        {
            return new GameConfiguration
            {
                Id = gameConfiguration.id,
                Name = gameConfiguration.name,
                MaxPaybackPercent = GsaRtpHelper.NormalizeRtp(gameConfiguration.maxPaybackPct),
                MinPaybackPercent = GsaRtpHelper.NormalizeRtp(gameConfiguration.minPaybackPct),
                MinDenomsEnabled = gameConfiguration.minDenomsEnabled,
                MaxDenomsEnabled = gameConfiguration.maxDenomsEnabledSpecified ? gameConfiguration.maxDenomsEnabled : (int?)null,
                Editable = gameConfiguration.editable,
                ConfigurationMapping = gameConfiguration.configurationMapList?.Select(Map).ToList() ?? new List<GameConfigurationMap>()
            };
        }

        private static GameConfigurationMap Map(c_configurationMap configurationMap)
        {
            return new GameConfigurationMap
            {
                Denomination = configurationMap.denomId,
                Variation = configurationMap.variationId,
                Enabled = configurationMap.enabled,
                Editable = configurationMap.editable,
                BetLinePresets = configurationMap.betLinePresetIdList.betLinePresetId,
                DefaultBetLinePreset = configurationMap.betLinePresetIdList.@default
            };
        }

        private static IEnumerable<SubGame> GetSubGames(c_subGameList subGameList)
        {
            if (subGameList is not null)
            {
                return (from subGame in subGameList.SubGame
                    let denominations = subGame.Denominations.Split(',').Select(long.Parse).ToList()
                    select new SubGame
                    {
                        TitleId = long.Parse(subGame.TitleId),
                        UniqueGameId = long.Parse(subGame.UniqueGameId),
                        Denominations = denominations,
                        CentralInfo = subGame.CdsInfoList?.cdsInfo.Select(Map).ToList() ?? Enumerable.Empty<CentralInfo>()
                    }).ToList();
            }

            return new List<SubGame>();
        }

        private static string GetVariationFromPaytableId(string paytableId)
        {
            const string delimiter = "_";

            // The variation identifier is the last value in the underscore separated paytable Id
            // This is in lieu of a separate field in the manifest.  This may need to change
            var index = paytableId.LastIndexOf(delimiter, StringComparison.InvariantCultureIgnoreCase);

            return index == -1 ? paytableId : paytableId.Substring(index + 1);
        }
    }
}