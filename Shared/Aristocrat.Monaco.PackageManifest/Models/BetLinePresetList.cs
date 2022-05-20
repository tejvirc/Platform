namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     A list of mappings between <see cref="BetOption"/> and <see cref="LineOption"/>.
    /// </summary>
    /// <remarks>
    ///     https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class BetLinePresetList : IEnumerable<BetLinePreset>
    {
        private readonly IEnumerable<BetLinePreset> _betLinePresets;

        /// <summary>
        ///     Creates a BetLinePresetList from a corresponding manifest object
        /// </summary>
        public BetLinePresetList(c_betLinePreset[] presets, BetOptionList betOptions, LineOptionList lineOptions)
        {
            if (presets == null || !presets.Any()
                || betOptions == null || !betOptions.Any()
                || lineOptions == null || !lineOptions.Any())
            {
                _betLinePresets = Enumerable.Empty<BetLinePreset>();
                return;
            }


            _betLinePresets =
                from p in presets
                select new BetLinePreset
                {
                    Id = p.id,
                    BetOption = betOptions.FirstOrDefault(o => o.Name.Equals(p.betOption)),
                    LineOption = lineOptions.FirstOrDefault(l => l.Name.Equals(p.lineOption)),
                };
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<BetLinePreset> GetEnumerator() => _betLinePresets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}