namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using System;
    using System.Collections.Generic;

    public class SubGameDetails : ISubGameDetails
    {
        public SubGameDetails(int uniqueId, string titleId, IList<Denomination> denominations)
        {
            Id = uniqueId;
            CdsTitleId = titleId;
            Denominations = denominations;
        }

        /// <inheritdoc />
        public string Version { get; set; }

        /// <inheritdoc />
        public DateTime ReleaseDate { get; set; }

        /// <inheritdoc />
        public DateTime InstallDate { get; set; }

        /// <inheritdoc />
        public bool Upgraded { get; set; }

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string ThemeId { get; set; }

        /// <inheritdoc />
        public string PaytableId { get; set; }

        /// <inheritdoc />
        public bool Active { get; set; }

        /// <inheritdoc />
        public int MaximumWagerCredits { get; set; }

        /// <inheritdoc />
        public int MinimumWagerCredits { get; set; }

        /// <inheritdoc />
        public long MaximumWinAmount { get; set; }

        /// <inheritdoc />
        public decimal MaximumPaybackPercent { get; set; }

        /// <inheritdoc />
        public decimal MinimumPaybackPercent { get; set; }

        /// <inheritdoc />
        public string PaytableName { get; set; }

        /// <inheritdoc />
        public string CdsThemeId { get; set; }

        /// <inheritdoc />
        public string CdsTitleId { get; set; }

        /// <inheritdoc />
        public IEnumerable<IDenomination> Denominations { get; set; }

        /// <inheritdoc />
        public IEnumerable<long> ActiveDenoms { get; set; }

        /// <inheritdoc />
        public IEnumerable<long> SupportedDenoms { get; set; }
    }
}
