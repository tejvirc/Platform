namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BingoServerSettingsModelConfiguration : IEntityTypeConfiguration<BingoServerSettingsModel>
    {
        public void Configure(EntityTypeBuilder<BingoServerSettingsModel> builder)
        {
            builder.ToTable(nameof(BingoServerSettingsModel));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.VoucherInLimit).IsRequired(false);
            builder.Property(x => x.BillAcceptanceLimit).IsRequired(false);
            builder.Property(x => x.TicketReprint).IsRequired(false);
            builder.Property(x => x.CaptureGameAnalytics).IsRequired(false);
            builder.Property(x => x.AlarmConfiguration).IsRequired(false);
            builder.Property(x => x.PlayerMayHideBingoCard).IsRequired(false);
            builder.Property(x => x.GameEndingPrize).IsRequired(false);
            builder.Property(x => x.PlayButtonBehavior).IsRequired(false);
            builder.Property(x => x.DisplayBingoCard).IsRequired(false);
            builder.Property(x => x.HideBingoCardWhenInactive).IsRequired(false);
            builder.Property(x => x.BingoCardPlacement).IsRequired(false);
            builder.Property(x => x.MaximumVoucherValue).IsRequired(false);
            builder.Property(x => x.MinimumJackpotValue).IsRequired(false);
            builder.Property(x => x.JackpotStrategy).IsRequired(false);
            builder.Property(x => x.JackpotAmountDetermination).IsRequired(false);
            builder.Property(x => x.PrintHandpayReceipt).IsRequired(false);
            builder.Property(x => x.LegacyBonusAllowed).IsRequired(false);
            builder.Property(x => x.AftBonusingEnabled).IsRequired(false);
            builder.Property(x => x.CreditsStrategy).IsRequired(false);
            builder.Property(x => x.BankId).IsRequired(false);
            builder.Property(x => x.ZoneId).IsRequired(false);
            builder.Property(x => x.Position).IsRequired(false);
            builder.Property(x => x.LapLevelIDs).IsRequired(false);
            builder.Property(x => x.BallCallService).IsRequired(false);
            builder.Property(x => x.BingoType).IsRequired(false);
            builder.Property(x => x.ReadySetGo).IsRequired(false);
            builder.Property(x => x.WaitingForPlayersMs).IsRequired(false);
            builder.Property(x => x.ServerGameConfiguration).IsRequired(false);
            builder.Property(x => x.GamesConfigurationText).IsRequired(false); ;
        }
    }
}
