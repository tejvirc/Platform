namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System.Collections.Generic;
    using Contracts.Configuration;

    internal class GameConfiguration : IGameConfiguration
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal MaximumPaybackPercent { get; set; }

        public decimal MinimumPaybackPercent { get; set; }

        public bool Editable { get; set; }

        public IEnumerable<IDenomToPaytable> Mapping { get; set; }

        public int? MaxDenomsEnabled { get; set; }

        public int MinDenomsEnabled { get; set; }
    }
}