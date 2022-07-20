namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ReportGameOutcomeMessage : IMessage
    {
        public long TransactionId { get; set; }

        public string MachineSerial { get; set; } = string.Empty;

        public long BetAmount { get; set; }

        public long TotalWin { get; set; }

        public long PaidAmount { get; set; }

        public long StartingBalance { get; set; }

        public long FinalBalance { get; set; }

        public int FacadeKey { get; set; }

        public long PresentationIndex { get; set; }

        public int GameEndWinEligibility { get; set; }

        public uint GameTitleId { get; set; }

        public uint ThemeId { get; set; }

        public int DenominationId { get; set; }

        public long GameSerial { get; set; }

        public string Paytable { get; set; } = string.Empty;

        public int JoinBall { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime JoinTime { get; set; }

        public IEnumerable<long> ProgressiveLevels { get; set; } = Enumerable.Empty<long>();

        public IEnumerable<CardPlayed> CardsPlayed { get; set; } = Enumerable.Empty<CardPlayed>();

        public IEnumerable<int> BallCall { get; set; } = Enumerable.Empty<int>();

        public IEnumerable<WinResult> WinResults { get; set; } = Enumerable.Empty<WinResult>();
    }
}