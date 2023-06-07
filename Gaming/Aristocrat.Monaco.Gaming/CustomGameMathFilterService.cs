namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts;
    using Kernel;
    using PackageManifest.Models;

    /// <summary>
    ///     Implementation of ICustomGameMathFilterService
    /// </summary>
    public class CustomGameMathFilterService : ICustomGameMathFilterService
    {
        private readonly IPropertiesManager _properties;
        private string _currentJurisdiction;

        public CustomGameMathFilterService(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICustomGameMathFilterService) };

        /// <inheritdoc />
        public void Initialize()
        {
            _currentJurisdiction = _properties.GetProperty(ApplicationConstants.JurisdictionId, "").ToString();
        }

        /// <inheritdoc />
        public void Filter(ref GameContent game)
        {
            if (game.JurisdictionRestrictions == null || !game.JurisdictionRestrictions.Any())
            {
                return;
            }

            var allJurisdictionRestrictions = game.JurisdictionRestrictions.ToList();
            //Can use the below method instead of the one above to test if operator menu makes the changes until manifest is updated.
            //var allJurisdictionRestrictions = TestGetJurisdictionRestrictions();

            var jurisdictionRestriction =
                allJurisdictionRestrictions.Find(
                    restriction => _currentJurisdiction.Contains(restriction.JurisdictionId));
            if (jurisdictionRestriction == null)
            {
                return;
            }
            //Filter based on variation Id
            var tempGameAtt = game.GameAttributes
                .Where(att => jurisdictionRestriction.AllowedVariationIds.Contains(att.VariationId)).ToList();
            // further filter Denoms and BetLinePreset
            tempGameAtt.ForEach(
                att =>
                {
                    att.Denominations = att.Denominations.Intersect(jurisdictionRestriction.AllowedDenoms);
                    att.BetLinePresetList = new BetLinePresetList(
                        att.BetLinePresetList
                            .Where(
                                betLinePreset =>
                                    jurisdictionRestriction.AllowedBetLinePresets.Contains(betLinePreset.Id)));
                });
            game.GameAttributes = tempGameAtt;
        }

        /// <summary>
        ///     This method is for test purposes so that we can see the changes in the operator menu until the manifest and games
        ///     are updated to include the jurisdiction restrictions.
        /// </summary>
        /// <returns>A sample list of jurisdiction restrictions</returns>
        private List<JurisdictionRestriction> TestGetJurisdictionRestrictions()
        {
            var jurisdictionRestrictions = new List<JurisdictionRestriction>
            {
                new()
                {
                    AllowedVariationIds = new List<string> { "01", "99" },
                    AllowedBetLinePresets = new List<int> { 0, 1, 7 },
                    AllowedDenoms = new List<long> { 1000, 2000, 10000 },
                    JurisdictionId = "JUR000090"
                },
                new()
                {
                    AllowedVariationIds = new List<string> { "01" },
                    AllowedBetLinePresets = new List<int> { 2 },
                    AllowedDenoms = new List<long> { 5000 },
                    JurisdictionId = "JUR000555"
                }
            };
            return jurisdictionRestrictions;
        }
    }
}