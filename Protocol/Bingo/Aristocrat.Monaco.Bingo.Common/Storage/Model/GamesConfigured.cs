namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class GamesConfigured
    {
        public IReadOnlyCollection<BingoGameConfiguration> Games { get; set; } = Array.Empty<BingoGameConfiguration>();
    }
}