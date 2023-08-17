namespace Aristocrat.Monaco.PackageManifest.Ati
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Models;
    using LevelTypes = Models.LevelType;

    /// <summary>
    ///     Implementation of the Progressive manifest
    /// </summary>
    public class ProgressiveManifest : IManifest<IEnumerable<ProgressiveDetail>>
    {
        private const string All = "ALL";
        private const string None = "NONE";

        /// <inheritdoc />
        public IEnumerable<ProgressiveDetail> Read(string file)
        {
            return !File.Exists(file) ? Enumerable.Empty<ProgressiveDetail>() : GetProgressiveDetails(Parse(file));
        }

        /// <inheritdoc />
        public IEnumerable<ProgressiveDetail> Read(Func<Stream> streamProvider)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ProgressiveDetail> GetProgressiveDetails(ProgressiveConfig config)
        {
            var details = new List<ProgressiveDetail>();

            foreach (var pack in config.ProgressivePack)
            {
                foreach (var progressive in pack.Progressive)
                {
                    var detail = new ProgressiveDetail
                    {
                        Id = Convert.ToInt32(progressive.id),
                        PackId = Convert.ToInt32(pack.id),
                        Name = pack.name,
                        LevelPack = progressive.levelPack,
                        Denomination = !string.IsNullOrEmpty(progressive.denom) ? progressive.denom.Split(',').ToArray() : new[] { All },
                        Variation = progressive.variation,
                        ReturnToPlayer = new ProgressiveRtp
                        {
                            ResetRtpMin = Convert.ToDecimal(progressive.resetRtpMin),
                            ResetRtpMax = Convert.ToDecimal(progressive.resetRtpMax),
                            IncrementRtpMin = Convert.ToDecimal(progressive.incrementRtpMin),
                            IncrementRtpMax = Convert.ToDecimal(progressive.incrementRtpMax),
                            BaseRtpAndResetRtpMin = progressive.baseResetRtpMin is null ? (decimal?)null : Convert.ToDecimal(progressive.baseResetRtpMin),
                            BaseRtpAndResetRtpMax = progressive.baseResetRtpMax is null ? (decimal?)null : Convert.ToDecimal(progressive.baseResetRtpMax),
                            BaseRtpAndResetRtpAndIncRtpMin = progressive.baseResetIncrementRtpMin is null ? (decimal?)null : Convert.ToDecimal(progressive.baseResetIncrementRtpMin),
                            BaseRtpAndResetRtpAndIncRtpMax = progressive.baseResetIncrementRtpMax is null ? (decimal?)null : Convert.ToDecimal(progressive.baseResetIncrementRtpMax)
                        },
                        Turnover = Convert.ToInt64(progressive.turnover),
                        ProgressiveType = progressive.poolControlTypeSpecified ? progressive.poolControlType : pack.poolControlType,
                        UseLevels = !string.IsNullOrEmpty(progressive.useLevels) ?
                            progressive.useLevels.Split(',').Select(l => l.Trim()).ToList() :
                            new[] { All }.ToList(),
                        CreationType = progressive.progType,
                        BetLinePreset = progressive.betLinePreset
                    };

                    detail.Levels = GetLevelDetails(config, pack, progressive, detail.UseLevels.ToList());
                    details.Add(detail);
                }
            }

            return details;
        }

        private IEnumerable<LevelDetail> GetLevelDetails(
            ProgressiveConfig config,
            ProgressivePackType progressivePack,
            ProgressiveType progressive,
            IReadOnlyCollection<string> useLevels)
        {
            var index = 0;
            var details = new List<LevelDetail>();

            var levelPacks = config.LevelPacks.Where(l => l.name.Equals(progressive.levelPack));

            foreach (var levelPack in levelPacks)
            {
                foreach (var level in GetApplicableLevels(levelPack, useLevels))
                {
                    var detail = new LevelDetail
                    {
                        LevelId = index++,
                        Name = level.name,
                        IncrementRate = Convert.ToDecimal(level.incrementRate, CultureInfo.InvariantCulture),
                        HiddenIncrementRate = Convert.ToDecimal(level.hiddenIncrementRate, CultureInfo.InvariantCulture),
                        Probability = Convert.ToDecimal(level.probability, CultureInfo.InvariantCulture),
                        MaximumValue = new ProgressiveValue(level.ceiling),
                        StartupValue = new ProgressiveValue(level.startUp),
                        AllowTruncation = bool.TryParse(level.allowTruncation, out var result) && result,
                        BonusValues = level.Bonuses?.ToDictionary(
                            b => b.key,
                            b => Convert.ToInt64(b.value)),
                        LevelType = progressivePack.sapFundingSpecified ? ToLevelType(progressivePack.sapFunding) :
                            level.sapFundingSpecified ? ToLevelType(level.sapFunding) : ToLevelType(level.flavor),
                        SapFundingType = GetSapFundingType(progressivePack, progressive, levelPack, level),
                        ProgressiveType = GetProgressiveType(progressivePack, progressive, levelPack, level),
                        FlavorType = GetFlavorType(progressivePack, progressive, levelPack, level),
                        Trigger = level.triggerSpecified ? level.trigger : triggerType.GAME,
                        LineGroup = Convert.ToInt32(level.lineGroup),
                        Rtp = Convert.ToDecimal(level.rtp)
                    };

                    details.Add(detail);
                }
            }

            return details;
        }

        private IEnumerable<LevelType> GetApplicableLevels(LevelPackType levelPack, IReadOnlyCollection<string> useLevels)
        {
            try
            {
                var levels = new List<LevelType>();

                // Check for ALL or NONE options first in 'useLevels'.
                var useLevelsOption = useLevels
                    .SingleOrDefault(
                        o => o.Equals(All, StringComparison.InvariantCultureIgnoreCase) ||
                            o.Equals(None, StringComparison.InvariantCultureIgnoreCase));

                if (useLevelsOption != null)
                {
                    if (useLevels.Count != 1)// We expect a single option if ALL or NONE is used.
                    {
                        // TODO : Does the exception message warrant a resource for localization?
                        throw new InvalidManifestException("Too many \"ALL\" or \"NONE\" entries included.");
                    }

                    if (useLevelsOption.Equals(All, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Defaulted to ALL if no 'useLevels' is specified in the progressive.xml.
                        levels.AddRange(levelPack.Level);
                    }
                    else
                    {
                        // NONE was included in 'useLevels'.
                        return Enumerable.Empty<LevelType>();
                    }
                }
                else
                {
                    // A list of level names were included in 'useLevels'; return those levels.
                    levels.AddRange(levelPack.Level.Where(level => useLevels.Contains(level.name)));
                }

                return levels;
            }
            catch (Exception e)
            {
                // Will not crash the platform; game will continue to load.
                throw new InvalidManifestException($"{e.Message}. \"useLevels\" in the progressive manifest is malformed.");
            }
        }


        private static ProgressiveConfig Parse(string file)
        {
            ProgressiveConfig config;

            try
            {
                config = ManifestUtilities.Parse<ProgressiveConfig>(file);
            }
            catch (Exception ex)
            {
                throw new InvalidManifestException("Failed to parse the progressive configuration file.", ex);
            }

            return config;
        }

        private sapFundingType GetSapFundingType(
            ProgressivePackType progressivePack,
            ProgressiveType progressive,
            LevelPackType levelPack,
            LevelType level)
        {
            var fundingType = sapFundingType.standard;

            if (level.sapFundingSpecified)
            {
                fundingType = level.sapFunding;
            }
            else if (levelPack.sapFundingSpecified)
            {
                fundingType = levelPack.sapFunding;
            }
            else if (progressive.sapFundingSpecified)
            {
                fundingType = progressive.sapFunding;
            }
            else if (progressivePack.sapFundingSpecified)
            {
                fundingType = progressivePack.sapFunding;
            }

            return fundingType;
        }

        private progressiveType GetProgressiveType(
            ProgressivePackType progressivePack,
            ProgressiveType progressive,
            LevelPackType levelPack,
            LevelType level)
        {
            var progressiveType = Ati.progressiveType.Unknown;

            if (level.poolControlTypeSpecified)
            {
                progressiveType = level.poolControlType;
            }
            else if (levelPack.poolControlTypeSpecified)
            {
                progressiveType = levelPack.poolControlType;
            }
            else if (progressive.poolControlTypeSpecified)
            {
                progressiveType = progressive.poolControlType;
            }
            else if (progressivePack.poolControlTypeSpecified)
            {
                progressiveType = progressivePack.poolControlType;
            }

            return progressiveType;
        }

        private static flavorType GetFlavorType(
            ProgressivePackType progressivePack,
            ProgressiveType progressive,
            LevelPackType levelPack,
            LevelType level)
        {
            var flavorType = Ati.flavorType.STANDARD;

            if (level.flavorSpecified)
            {
                flavorType = level.flavor;
            }
            else if (levelPack.flavorSpecified)
            {
                flavorType = levelPack.flavor;
            }
            else if (progressive.flavorSpecified)
            {
                flavorType = progressive.flavor;
            }
            else if (progressivePack.flavorSpecified)
            {
                flavorType = progressivePack.flavor;
            }

            return flavorType;
        }

        private static LevelTypes ToLevelType(flavorType levelType)
        {
            return levelType switch
            {
                flavorType.STANDARD => LevelTypes.Standard,
                flavorType.BULK_CONTRIBUTION => LevelTypes.Bulk,
                flavorType.VERTEX_MYSTERY => LevelTypes.Mystery,
                flavorType.HOSTCHOICE => LevelTypes.HostChoice,
                _ => LevelTypes.Standard
            };
        }

        private static LevelTypes ToLevelType(sapFundingType packType)
        {
            return packType switch
            {
                sapFundingType.standard => LevelTypes.Standard,
                sapFundingType.line_based => LevelTypes.Standard,
                sapFundingType.ante => LevelTypes.BulkWithAnteBet,
                sapFundingType.line_based_ante => LevelTypes.BulkWithAnteBet,
                sapFundingType.bulk_only => LevelTypes.Bulk,
                _ => LevelTypes.Standard
            };
        }
    }
}