namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    public class BingoServerSettingsModelConfiguration : EntityTypeConfiguration<BingoServerSettingsModel>
    {
        public BingoServerSettingsModelConfiguration()
        {
            ToTable(nameof(BingoServerSettingsModel));
            HasKey(x => x.Id);
            Property(x => x.VoucherInLimit).IsOptional();
            Property(x => x.BillAcceptanceLimit).IsOptional();
            Property(x => x.TicketReprint).IsOptional();
            Property(x => x.CaptureGameAnalytics).IsOptional();
            Property(x => x.AlarmConfiguration).IsOptional();
            Property(x => x.PlayerMayHideBingoCard).IsOptional();
            Property(x => x.GameEndingPrize).IsOptional();
            Property(x => x.PlayButtonBehavior).IsOptional();
            Property(x => x.DisplayBingoCard).IsOptional();
            Property(x => x.HideBingoCardWhenInactive).IsOptional();
            Property(x => x.BingoCardPlacement).IsOptional();
            Property(x => x.MaximumVoucherValue).IsOptional();
            Property(x => x.MinimumJackpotValue).IsOptional();
            Property(x => x.JackpotStrategy).IsOptional();
            Property(x => x.JackpotAmountDetermination).IsOptional();
            Property(x => x.PrintHandpayReceipt).IsOptional();
            Property(x => x.LegacyBonusAllowed).IsOptional();
            Property(x => x.AftBonusingEnabled).IsOptional();
            Property(x => x.CreditsStrategy).IsOptional();
            Property(x => x.BankId).IsOptional();
            Property(x => x.ZoneId).IsOptional();
            Property(x => x.Position).IsOptional();
            Property(x => x.LapLevelIDs).IsOptional();
            Property(x => x.GameTitles).IsOptional();
            Property(x => x.BonusGames).IsOptional();
            Property(x => x.EvaluationTypePaytable).IsOptional();
            Property(x => x.ThemeSkins).IsOptional();
            Property(x => x.QuickStopMode).IsOptional();
            Property(x => x.PaytableIds).IsOptional();
            Property(x => x.BallCallService).IsOptional();
            Property(x => x.BingoType).IsOptional();
            Property(x => x.ReadySetGo).IsOptional();
            Property(x => x.WaitingForPlayersMs).IsOptional();
        }
    }
}
