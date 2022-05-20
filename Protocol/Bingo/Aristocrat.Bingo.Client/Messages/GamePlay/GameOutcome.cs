namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class GameOutcome : IResponse
    {
        public GameOutcome(
            ResponseCode code,
            string machineSerial,
            long totalWin,
            string progressiveLevels,
            int facadeKey,
            uint gameTitleId,
            uint themeId,
            int denominationId,
            long gameSerial,
            string statusMessage,
            bool isSuccessful,
            string paytable,
            int gameEndWinEligibility,
            IEnumerable<CardPlayed> cardsPlayed,
            IEnumerable<int> ballCall,
            IEnumerable<WinResult> winResults,
            bool isFinal)
        {
            ResponseCode = code;
            MachineSerial = machineSerial;
            TotalWin = totalWin;
            ProgressiveLevels = progressiveLevels;
            FacadeKey = facadeKey;
            GameTitleId = gameTitleId;
            ThemeId = themeId;
            DenominationId = denominationId;
            GameSerial = gameSerial;
            StatusMessage = statusMessage;
            IsSuccessful = isSuccessful;
            Paytable = paytable;
            GameEndWinEligibility = gameEndWinEligibility;
            CardsPlayed = cardsPlayed.ToList();
            BallCall = ballCall.ToList();
            WinResults = winResults.ToList();
            IsFinal = isFinal;
        }

        public GameOutcome(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }

        public string MachineSerial { get; }

        public long TotalWin { get; }

        public string ProgressiveLevels { get; }

        public int FacadeKey { get; }

        public uint GameTitleId { get; }

        public uint ThemeId { get; }

        public int DenominationId { get; }

        public long GameSerial { get; }

        public string StatusMessage { get; }

        public bool IsSuccessful { get; }

        public string Paytable { get; }

        public int GameEndWinEligibility { get; }

        public IReadOnlyCollection<CardPlayed> CardsPlayed { get; }

        public IReadOnlyCollection<int> BallCall { get; }

        public IReadOnlyCollection<WinResult> WinResults { get; }

        public bool IsFinal { get; }
    }
}