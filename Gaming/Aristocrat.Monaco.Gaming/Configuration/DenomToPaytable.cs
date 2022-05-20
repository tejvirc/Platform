namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System.Collections.Generic;
    using Contracts.Configuration;

    internal class DenomToPaytable : IDenomToPaytable
    {
        public bool Active { get; set; }

        public long Denomination { get; set;  }

        public string VariationId { get; set; }

        public bool EnabledByDefault { get; set; }

        public bool Editable { get; set; }

        public int DefaultBetLinePresetId { get; set; }

        public IEnumerable<int> BetLinePresets { get; set; }
    }
}