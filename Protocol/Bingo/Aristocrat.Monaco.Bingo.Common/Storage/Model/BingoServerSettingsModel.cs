namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Diagnostics.CodeAnalysis;
    using Monaco.Common.Storage;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when reading from the database and will set via the server in the future")]
    public class BingoServerSettingsModel : BaseEntity
    {
        public long? VoucherInLimit { get; set; }

        public long? BillAcceptanceLimit { get; set; }

        public bool? TicketReprint { get; set; }

        public bool? CaptureGameAnalytics { get; set; }

        public bool? AlarmConfiguration { get; set; }

        public string PlayerMayHideBingoCard { get; set; }

        public GameEndWinStrategy? GameEndingPrize { get; set; }

        public string PlayButtonBehavior { get; set; }

        public bool? DisplayBingoCard { get; set; }

        public  bool? HideBingoCardWhenInactive { get; set; }

        public string BingoCardPlacement { get; set; }

        public long? MaximumVoucherValue { get; set; }

        public long? MinimumJackpotValue { get; set; }

        public JackpotStrategy JackpotStrategy { get; set; }

        public JackpotDetermination JackpotAmountDetermination { get; set; }

        public bool? PrintHandpayReceipt { get; set; }

        public string LegacyBonusAllowed { get; set; }

        public bool? AftBonusingEnabled { get; set; }

        public CreditsStrategy? CreditsStrategy { get; set; }

        public string BankId { get; set; }

        public string ZoneId { get; set; }

        public string Position { get; set; }

        public string LapLevelIDs { get; set; }

        public string GameTitles { get; set; }

        public string BonusGames { get; set; }

        public PaytableEvaluation EvaluationTypePaytable { get; set; }

        public string ThemeSkins { get; set; }

        public bool? QuickStopMode { get; set; }

        public string PaytableIds { get; set; }

        public string BallCallService { get; set; }

        public BingoType BingoType { get; set; }

        public ContinuousPlayMode? ReadySetGo { get; set; }

        public long? WaitingForPlayersMs { get; set; }
    }
}